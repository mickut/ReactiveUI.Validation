// Copyright (c) 2020 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Validation.Tests.Models
{
    /// <summary>
    /// Mocked View.
    /// </summary>
    public class TestView : IViewFor<TestViewModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestView"/> class.
        /// </summary>
        /// <param name="viewModel">ViewModel instance of type <see cref="TestViewModel"/>.</param>
        public TestView(TestViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        /// <inheritdoc/>
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = value as TestViewModel;
        }

        /// <inheritdoc/>
        public TestViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the Name Label which emulates a Text property (eg. Entry in Xamarin.Forms).
        /// </summary>
        public string NameLabel { get; set; }

        /// <summary>
        /// Gets or sets the Name2 Label which emulates a Text property (eg. Entry in Xamarin.Forms).
        /// </summary>
        public string Name2Label { get; set; }

        /// <summary>
        /// Gets or sets the NameError Label which emulates a Text property (eg. Entry in Xamarin.Forms).
        /// </summary>
        public string NameErrorLabel { get; set; }

        /// <summary>
        /// Gets the error label which is represented by a container.
        /// </summary>
        public TestControl NameErrorContainer { get; } = new TestControl();
    }
}
