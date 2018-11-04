// 
//  ROMExplorer
// 
//  Copyright 2018 Martin Gerczuk
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software 
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ROMExplorer
{
    internal class DoubleClickBehavior
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command", typeof(ICommand), typeof(DoubleClickBehavior),
            new PropertyMetadata(default(ICommand), CommandOnChanged));

        private static void CommandOnChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (!(obj is ContentControl control))
                throw new ArgumentException("DoubleClickBehavior.Command can only be attached to ContentControl.");

            var command = (ICommand) args.NewValue;

            control.MouseDoubleClick += (sender, eventArgs) =>
            {
                if (command.CanExecute(control.Content)) command.Execute(control.Content);
            };
        }

        public static void SetCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(CommandProperty, value);
        }

        public static ICommand GetCommand(DependencyObject element)
        {
            return (ICommand) element.GetValue(CommandProperty);
        }
    }
}