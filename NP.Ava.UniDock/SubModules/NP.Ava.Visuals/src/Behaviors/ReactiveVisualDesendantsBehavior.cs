﻿using Avalonia;
using Avalonia.VisualTree;
using NP.Concepts.Behaviors;

namespace NP.Ava.Visuals.Behaviors
{
    public class ReactiveVisualDesendantsBehavior : FlattenReactiveTreeBehavior<Visual>
    {
        public ReactiveVisualDesendantsBehavior(Visual root) 
            : 
            base(root, visual => visual.GetVisualChildren())
        {
        }
    }
}
