﻿// (c) Nick Polyak 2018 - http://awebpros.com/
// License: Apache License 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.

namespace NP.IoC.CommonImplementations
{
    public sealed class FullContainerItemResolvingKey<TKey>
    {
        public Type ResolvingType { get; }

        // allows resolution by object 
        // without this, a single type would always be 
        // resolved in a single way. 
        public TKey? KeyObject { get; }


        public FullContainerItemResolvingKey(Type resolvingType, TKey keyObject)
        {
            this.ResolvingType = resolvingType;
            this.KeyObject = keyObject;
        }

        public override bool Equals(object? obj)
        {
            if (obj is FullContainerItemResolvingKey<TKey> target)
            {
                return this.ResolvingType.ObjEquals(target.ResolvingType) &&
                       this.KeyObject.ObjEquals(target.KeyObject);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.ResolvingType.GetHashCode() ^ this.KeyObject.GetHashCodeExtension();
        }

        public override string ToString()
        {
            string result = $"{ResolvingType.Name}";

            if (KeyObject != null)
            {
                result += $": {KeyObject.ToStr()}";
            }

            return result;
        }
    }
}
