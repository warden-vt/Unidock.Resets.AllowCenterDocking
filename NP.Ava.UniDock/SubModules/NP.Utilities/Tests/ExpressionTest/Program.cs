﻿using ExpressionTest;
using System.Reflection;
using NP.Utilities.Expressions;
using System.Linq.Expressions;
using NP.Utilities;

public class Program
{
    public static int Plus(int i1, int i2)
    {
        return i1 + i2;
    }

    public static void PlusRef(ref int referenceInt, int number2)
    {
        referenceInt += number2;
    }

    public static int PlusRefWithReturn(ref int referenceInt, int number2)
    {
        referenceInt += number2;

        return referenceInt;
    }

    public static void PlusOut(int i1, int i2, out int result)
    {
        result = i1 + i2;
    }

    public static int PlusOutWithReturn(int i1, int i2, out int result)
    {
        result = i1 + i2;

        return result;
    }

    internal static int Square(int i)
    {
        return i * i;
    }

    private static void VoidMethodTest()
    {
        Counter counter = new Counter();

        MethodInfo methodInfo =
            typeof(Counter).GetMethod(nameof(Counter.UpCount));

        Action upCountAction = (Action)methodInfo.GetCompiledMethodCallLambda(counter);


        upCountAction();
        upCountAction();
        upCountAction();

        Console.WriteLine(counter.Count); //3
    }

    public static void MultiArgMethodWithReturnTest()
    {
        MethodInfo plusMethodInfo = typeof(Program).GetMethod(nameof(Program.Plus))!;

        Func<object[], object> func = plusMethodInfo.GetParamArrayLambdaForReturningMethod();

        int result = (int)func.Invoke(new object[] { 10, 2 });

        Console.WriteLine(result);
    }

    public static void TestBlockExprWithReturn()
    {
        ParameterExpression variableExpr = Expression.Variable(typeof(int), "i");
        Expression assignExpr = Expression.Assign(variableExpr, Expression.Constant(10));
        Expression blockExpr = Expression.Block(
            typeof(int), // return type
            new ParameterExpression[] { variableExpr },
            assignExpr); // last expression is returned (apparently if there is a return type)

        ///.Block(System.Int32 $i) {
        ///     $i = 10
        /// }

        Func<int> f = Expression.Lambda<Func<int>>(blockExpr).Compile();

        int result = f.Invoke();

        Console.WriteLine(result);
    }

    public static void TestCallReturningMethodExpr()
    {
        Type type = typeof(Program);

        MethodInfo methodInfo = type.GetMethod(nameof(Program.Square))!;

        ParameterExpression inputParamExpression = Expression.Parameter(typeof(int), "i");

        var callSquareExpr = Expression.Call(methodInfo, new[] { inputParamExpression });

        Func<int, int> squared = Expression.Lambda<Func<int, int>>(callSquareExpr, inputParamExpression).Compile();

        Console.Write(squared(10)); // prints 100 (10 * 10)
    }

    public static void TwiceTheInputByRef(ref int x)
    {
        x = x * 2;
    }

    public static void RefSampleFromStackOverflow()
    {
        Type type = typeof(Program);

        MethodInfo methodInfo =
            type.GetMethod("TwiceTheInputByRef")!;

        var inputVar = Expression.Variable(typeof(int), "input");
        var blockExp =
            Expression.Block(
                    new[] { inputVar }
                    , Expression.Assign(inputVar, Expression.Constant(10))
                    , Expression.Call(methodInfo, inputVar)
                    , inputVar
                    );

        // returns 20
        var result = Expression.Lambda<Func<int>>(blockExp).Compile()();
    }

    public static void RefSampleWithInputArgs()
    {

        Type type = typeof(Program);

        MethodInfo plusRefMethodInfo = type.GetMethod(nameof(Program.PlusRef))!;

        ParameterExpression inputParamExpression1 = Expression.Parameter(typeof(int), "i1");
        ///$i1

        ParameterExpression variableExpr = Expression.Variable(typeof(int), "referencedInt");
        ///$referencedInt

        Expression assignExpr = Expression.Assign(variableExpr, inputParamExpression1);
        ///$referencedInt = $i1

        ParameterExpression inputParamExpression2 = Expression.Parameter(typeof(int), "i2");
        ///$i2

        var callPlusRefExpr = Expression.Call(plusRefMethodInfo, variableExpr, inputParamExpression2);
        ///.Call Program.PlusRef(
        ///     $referencedInt,
        ///     $i2)

        Expression body =
            Expression.Block
            (
                typeof(int),
                new ParameterExpression[] { variableExpr },
                new Expression[] { assignExpr, callPlusRefExpr, variableExpr });
        ///.Block(System.Int32 $referencedInt) { // this is where the block's variables are defined (ref or not does not matter)
        ///    $referencedInt = $i1; // i1 is the variable coming from the input parameters
        ///    .Call Program.PlusRef(
        ///        $referencedInt,
        ///        $i2);             // i2 is the variagble coming from input parameters
        ///    $referencedInt   // last line specifies what the block returns.
        ///}       

        var resultExpression = Expression.Lambda<Func<int, int, int>>
            (
                body,
                new ParameterExpression[]
                {
                    inputParamExpression1,
                    inputParamExpression2 });
        ///.Lambda #Lambda1<System.Func`3[System.Int32,System.Int32,System.Int32]>(
        ///    System.Int32 $i1,
        ///    System.Int32 $i2) {
        ///    .Block(System.Int32 $referencedInt) {
        ///        $referencedInt = $i1;
        ///        .Call Program.PlusRef(
        ///            $referencedInt,
        ///            $i2);
        ///        $referencedInt
        ///    }
        ///}

        Func<int, int, int> f = resultExpression.Compile();

        int i1 = 3, i2 = 4;
        int i = f(i1, i2); // i = 7

        Console.WriteLine(i);
    }

    public static void OutSampleWithInputArgs()
    {

        Type type = typeof(Program);

        MethodInfo plusOutMethodInfo = type.GetMethod(nameof(Program.PlusOut))!;

        ParameterExpression inputParamExpression1 = Expression.Parameter(typeof(int), "i1");
        ParameterExpression inputParamExpression2 = Expression.Parameter(typeof(int), "i2");
        ParameterExpression resultExpr = Expression.Variable(typeof(int), "result");

        var callPlusOutExpr =
            Expression.Call
            (
                plusOutMethodInfo,
                inputParamExpression1,
                inputParamExpression2,
                resultExpr);
        ///.Call Program.PlusOut
        /// (
        ///      $i1,
        ///      $i2,
        ///      $result)


        Expression body =
            Expression.Block
            (
                typeof(int),
                new ParameterExpression[] { resultExpr },
                new Expression[] { callPlusOutExpr, resultExpr });
        ///.Block(System.Int32 $result) {
        ///    .Call Program.PlusOut
        ///     (
        ///         $i1,
        ///         $i2,
        ///         $result); 
        ///    $result
        ///}


        Expression<Func<int, int, int>> lambdaExpression =
            Expression.Lambda<Func<int, int, int>>
            (
                body,
                new ParameterExpression[] { inputParamExpression1, inputParamExpression2 });
        ///.Lambda #Lambda1<System.Func`3[System.Int32,System.Int32,System.Int32]>(
        ///    System.Int32 $i1,
        ///    System.Int32 $i2) {
        ///    .Block(System.Int32 $result) {
        ///        .Call Program.PlusOut(
        ///            $i1,
        ///            $i2,
        ///            $result);
        ///        $result
        ///    }
        ///}

        Func<int, int, int> f = lambdaExpression.Compile();

        int i1 = 3, i2 = 4;
        int result = f(i1, i2); // i = 7

        Console.WriteLine(result);
    }

    public static void RefSampleWithInputArgsConversion()
    {

        Type type = typeof(Program);

        MethodInfo plusRefMethodInfo = type.GetMethod(nameof(Program.PlusRef))!;

        ParameterExpression inputParamExpression1 = Expression.Parameter(typeof(int), "i1");
        ParameterExpression variableExpr = Expression.Variable(typeof(int), "referencedInt");
        Expression assignExpr = Expression.Assign(variableExpr, inputParamExpression1);

        ParameterExpression inputParamExpression2 = Expression.Parameter(typeof(int), "i2");

        var callPlusRefExpr = Expression.Call(plusRefMethodInfo, variableExpr, inputParamExpression2);
        ///.Call Program.PlusRef(
        ///    $referencedInt,
        ///    $i2)

        Expression body =
            Expression.Block
            (
                typeof(int), // return type
                new ParameterExpression[] { variableExpr },
                new Expression[] { assignExpr, callPlusRefExpr, variableExpr });
        ///.Block(System.Int32 $referencedInt) { // this is where the block's variables are defined (ref or not does not matter)
        ///        $referencedInt = $i1; // i1 is the variable coming from the input parameters
        ///        .Call Program.PlusRef(
        ///            $referencedInt,
        ///            $i2);             // i2 is the variagble coming from input parameters
        ///        $referencedInt   // last line specifies what the block returns.
        /// }       
        /// 

        Expression<Func<object[], object[]>> lambdaExpression =
            Expression.Lambda<Func<object[], object[]>>(body, new ParameterExpression[] { inputParamExpression1, inputParamExpression2 });

        Func<object[], object[]> f = lambdaExpression.Compile();

        int i1 = 3, i2 = 4;
        object[] result = f(new object[] { 3, 4 }); // i = 7

        Console.WriteLine(result[0]);
    }


    public static void UsingParamValues()
    {
        Type type = typeof(Program);

        ParameterExpression inputArrayParamsExpression = 
            Expression.Parameter(typeof(object[]), MethodCaller.INPUT_PARAM_NAME);
        ParameterExpression outputArrayParamsExpression = 
            Expression.Parameter(typeof(object[]), MethodCaller.OUTPUT_PARAM_NAME);

        ParameterExpression returnParamExpression =
            Expression.Parameter(typeof(object[]), MethodCaller.RETURN_PARAM_NAME);

        MethodInfo plusRefMethodInfo = type.GetMethod(nameof(Program.Plus))!;

        var paramValues =
            plusRefMethodInfo
                .GetParameters()
                .Select(p => new ParamValue(p, inputArrayParamsExpression, outputArrayParamsExpression));

        ParamValue returnParamValue = new ParamValue(plusRefMethodInfo.ReturnParameter, inputArrayParamsExpression, outputArrayParamsExpression, returnParamExpression);


        paramValues = paramValues.UnionSingle(returnParamValue).ToArray();
        int inputIdx = 0;
        int outputIdx = 0;

        foreach (var paramValue in paramValues)
        {
            if (paramValue.IsIn)
            {
                paramValue.InputIdx = inputIdx;
                inputIdx++;
            }

            if (paramValue.IsOut)
            {
                paramValue.OutputIdx = outputIdx;
                outputIdx++;
            }
        }


        IEnumerable<ParamValue> inputParams = paramValues.Where(p => p.IsIn);
        IEnumerable<ParamValue> outputParams = paramValues.Where(p => p.IsOut);

        var callExpression = 
           Expression
                .Assign
                (
                    returnParamValue.ReturnArrayAccessExpr!, 
                    Expression.Convert(
                        Expression.Call
                        (
                            plusRefMethodInfo,
                            inputParams.Select(p => p.InputParamExpression)!), 
                        typeof(object)
                    )
                );

        ///.Call Program.Plus(
        ///    $referenceInt,
        ///    $number2)

        IEnumerable<Expression> assignInputExpressions =
            paramValues
                .Where(p => p.AssignInputValueExpression != null)
                .Select(p => p.AssignInputValueExpression)!;

        IEnumerable<Expression> assignOutputExpressions =
            outputParams.Select(p => p.AssignOutputValueExpression)!;

        IEnumerable<Expression> returnExpressions =
            paramValues.Where(p => p.IsReturn)
                        .Select(p => p.ReturnArrayAccessExpr)!;

        Expression body =
            Expression.Block
            (
                typeof(object),
                outputParams.Select(p => p.Expr).ToArray()!, // variables
                assignInputExpressions
                    .Union(new[] { callExpression })
                    .Union(assignOutputExpressions)
                    .Union(returnExpressions).ToArray()/*.Union(new[] {resultParamExpression})*/);

        ///.Block(System.Int32 $referenceInt) {
        ///    $referenceInt = (System.Int32)$__Input__[0];
        ///    .Call Program.PlusRef(
        ///        $referenceInt,
        ///        (System.Int32)$__Input__[1]);
        ///    $__Output__[0] = (System.Object)$referenceInt
        ///}

        Expression<Action<object[], object[], object[]>> lambdaExpression =
            Expression.Lambda<Action<object[], object[], object[]>>
            (
                body,
                new ParameterExpression[] 
                { 
                    inputArrayParamsExpression, 
                    outputArrayParamsExpression,
                    returnParamExpression}
            );
        ///.Lambda #Lambda1<System.Action`2[System.Object[],System.Object[]]>(
        ///    System.Object[] $__Input__,
        ///    System.Object[] $__Output__) {
        ///    .Block(System.Int32 $referenceInt) {
        ///        $referenceInt = (System.Int32)$__Input__[0];
        ///        .Call Program.PlusRef(
        ///            $referenceInt,
        ///            (System.Int32)$__Input__[1]);
        ///        $__Output__[0] = (System.Object)$referenceInt
        ///    }
        ///}

        Action<object[], object[], object[]> lambda = lambdaExpression.Compile();

        var input = new object[] { 13, 2 };
        var output = new object[1];

        lambda(input, new object[] { }, output);

        Console.WriteLine($"Result = {output[0]}");
    }

    public static void Main(string[] args)
    {
        // VoidMethodTest();

        //MultiArgMethodWithReturnTest();

        //TestBlockExprWithReturn();

        //RefSampleFromStackOverflow();

        //RefSampleWithInputArgs();

        //OutSampleWithInputArgs();

        //RefSampleWithInputArgsConversion();

        //UsingParamValues();


        #region MethodCaller
        ///-----------------------------------------------------------------------------
        Type type = typeof(Program);

        MethodInfo plusRefMethodInfo = type.GetMethod(nameof(Program.PlusRef))!;
        MethodCaller methodCaller = new MethodCaller(plusRefMethodInfo);

        methodCaller.SetInputValue("referenceInt", 123);
        methodCaller.SetInputValue("number2", 34);

        methodCaller.Call();

        int refInt = (int)methodCaller.GetOutputValue("referenceInt")!;
        Console.WriteLine(refInt);
        
        ///-----------------------------------------------------------------------------
        MethodInfo plusReturnMethodInfo =
            type.GetMethod(nameof(Program.Plus))!;
        MethodCaller outMethodCaller = new MethodCaller(plusReturnMethodInfo);
        outMethodCaller.SetInputValue("i1", 12);
        outMethodCaller.SetInputValue("i2", 4);

        outMethodCaller.Call();

        int returnInt = (int)outMethodCaller.GetReturnValue()!;

        Console.WriteLine(returnInt);

        ///-----------------------------------------------------------------------------
        MethodInfo plusOutMethodInfo =
            type.GetMethod(nameof(Program.PlusOut))!;

        MethodCaller plusOutMethodCaller = new MethodCaller(plusOutMethodInfo);
        plusOutMethodCaller.SetInputValue("i1", 12);
        plusOutMethodCaller.SetInputValue("i2", 4);

        plusOutMethodCaller.Call();

        int outInt = (int)plusOutMethodCaller.GetOutputValue("result");
        //returnInt = (int)outMethodCaller.GetReturnValue()!;

        Console.WriteLine($"outInt={outInt}");

        ///-----------------------------------------------------------------------------
        MethodInfo plusOutWithReturnMethodInfo =
            type.GetMethod(nameof(Program.PlusOutWithReturn))!;

        MethodCaller plusOutWithReturnMethodCaller = new MethodCaller(plusOutWithReturnMethodInfo);
        plusOutWithReturnMethodCaller.SetInputValue("i1", 122);
        plusOutWithReturnMethodCaller.SetInputValue("i2", 42);

        plusOutWithReturnMethodCaller.Call();

        outInt = (int)plusOutWithReturnMethodCaller.GetOutputValue("result");
        returnInt = (int)plusOutWithReturnMethodCaller.GetReturnValue()!;

        Console.WriteLine($"outInt={outInt}, result={returnInt}");

        ///-----------------------------------------------------------------------------
        MethodInfo plusRefWithReturnMethodInfo =
            type.GetMethod(nameof(Program.PlusRefWithReturn))!;

        MethodCaller plusRefWithReturnMethodCaller = new MethodCaller(plusRefWithReturnMethodInfo);
        plusRefWithReturnMethodCaller.SetInputValue("referenceInt", 1222);
        plusRefWithReturnMethodCaller.SetInputValue("number2", 422);

        plusRefWithReturnMethodCaller.Call();

        outInt = (int)plusRefWithReturnMethodCaller.GetOutputValue("referenceInt");
        returnInt = (int)plusRefWithReturnMethodCaller.GetReturnValue()!;

        Console.WriteLine($"outInt={outInt}, result={returnInt}");

        #endregion MethodCaller
    }
}

