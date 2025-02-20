﻿// (c) Nick Polyak 2021 - http://awebpros.com/
// License: MIT License (https://opensource.org/licenses/MIT)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.

using NP.Utilities;
using System;
using System.Collections.Generic;

namespace NP.Concepts.Behaviors
{
    public class MimicCollectionBehavior<TInput, TOutput, TOutputCollection> : VMBase
        where TOutputCollection : IList<TOutput>, new()
    {
        private IDisposable? _behavior;

        private IEnumerable<TInput> _inputCollection;
        public IEnumerable<TInput> InputCollection
        {
            get => _inputCollection;

            set
            {
                if (ReferenceEquals(_inputCollection, value))
                    return;

                _behavior?.Dispose();

                _inputCollection = value;

                _behavior = _inputCollection?.AddDetailedBehavior(OnInputItemAdded, OnInputItemRemoved);

                _outputCollection = (_inputCollection == null) ? null : new TOutputCollection();

                OnPropertyChanged(nameof(OutputCollection));
            }
        }

        public Func<TInput, TOutput> InputToOutputConverter { get; }

        private void OnInputItemAdded(IEnumerable<TInput> inputCollection, TInput inputItem, int idx)
        {
            TOutput outputItem = InputToOutputConverter.Invoke(inputItem);

            _outputCollection.Insert(idx, outputItem);
        }

        private void OnInputItemRemoved(IEnumerable<TInput> inputCollection, TInput inputItem, int idx)
        {
            _outputCollection.RemoveAt(idx);
        }

        private IList<TOutput> _outputCollection;
        public IEnumerable<TOutput> OutputCollection => _outputCollection;

        public MimicCollectionBehavior(Func<TInput, TOutput> _inputToOutputConverter = null)
        {
            InputToOutputConverter = _inputToOutputConverter;

            if (InputToOutputConverter == null)
            {
                InputToOutputConverter = (input) => (TOutput)(object)input;
            }
        }
    }
}
