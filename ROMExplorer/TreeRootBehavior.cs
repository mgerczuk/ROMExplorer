﻿// 
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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ROMExplorer
{
    public class TreeRootBehavior : Behavior<TreeView>
    {
        public static readonly DependencyProperty RootProperty = DependencyProperty.Register(
            "Root", typeof(DiscDirectoryInfoTreeItemViewModel), typeof(TreeRootBehavior),
            new PropertyMetadata(default(DiscDirectoryInfoTreeItemViewModel), RootOnPropertyChanged));

        public DiscDirectoryInfoTreeItemViewModel Root
        {
            get => (DiscDirectoryInfoTreeItemViewModel) GetValue(RootProperty);
            set => SetValue(RootProperty, value);
        }

        private static void RootOnPropertyChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            var treeView = ((TreeRootBehavior) dependencyObject).AssociatedObject;
            var viewModel = (DiscDirectoryInfoTreeItemViewModel)args.NewValue;

            if (viewModel != null)
            {
                viewModel.IsExpanded = true;
                viewModel.IsSelected = true;
                treeView.ItemsSource = new[] {viewModel};
            }
            else
                treeView.ItemsSource = null;
        }
    }
}