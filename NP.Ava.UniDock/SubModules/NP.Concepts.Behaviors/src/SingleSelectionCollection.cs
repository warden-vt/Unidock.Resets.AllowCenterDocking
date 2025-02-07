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
using System.ComponentModel;
using System.Linq;

namespace NP.Concepts.Behaviors
{
    public class SingleSelectionCollection<TData> : 
        List<SelectableItemWithData<TData>>, 
        INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public SelectableItemWithData<TData> SelectedItem
        {
            get => _behavior.TheSelectedItem;
            set => _behavior.TheSelectedItem = value;
        }

        SingleSelectionBehavior<SelectableItemWithData<TData>> _behavior;
        public SingleSelectionCollection(IEnumerable<TData> values)
        {
            this.AddRange(values.Select(val => val.ToSelectableItem()).ToList());

            _behavior = new SingleSelectionBehavior<SelectableItemWithData<TData>>
            {
                TheCollection = this
            };

            _behavior.PropertyChanged += _behavior_PropertyChanged;
        }

        private void _behavior_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_behavior.TheSelectedItem))
            {
                OnPropChanged(nameof(SelectedItem));
            }
        }
    }

    public class SingleSelectionNonNullableCollection<TData> : SingleSelectionCollection<TData>
        where TData : struct
    {
        public SingleSelectionNonNullableCollection(IEnumerable<TData> values) : base(values)
        {
        }

        public TData? SelectedData
        {
            get => SelectedItem?.Data;

            set
            {
                this.SelectedItem = this.FirstOrDefault(item => item.Data.ObjEquals(value));
            }
        }
    }
}
