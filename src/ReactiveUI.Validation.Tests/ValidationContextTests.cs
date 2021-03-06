// Copyright (c) 2020 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reactive.Concurrency;
using ReactiveUI.Validation.Components;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Tests.Models;
using Xunit;

namespace ReactiveUI.Validation.Tests
{
    /// <summary>
    /// Tests for <see cref="ValidationContext"/>.
    /// </summary>
    public class ValidationContextTests
    {
        /// <summary>
        /// Verifies that a <see cref="ValidationContext"/> without validations is valid.
        /// </summary>
        [Fact]
        public void EmptyValidationContextIsValid()
        {
            var vc = new ValidationContext(ImmediateScheduler.Instance);

            Assert.True(vc.IsValid);
            Assert.Equal(0, vc.Text.Count);
        }

        /// <summary>
        /// Verifies that validations can be added in the <see cref="ValidationContext"/>.
        /// </summary>
        [Fact]
        public void CanAddValidationComponentsTest()
        {
            var vc = new ValidationContext(ImmediateScheduler.Instance);

            var invalidName = string.Empty;

            var vm = new TestViewModel { Name = "valid" };

            var v1 = new BasePropertyValidation<TestViewModel, string>(
                vm,
                v => v.Name,
                s => !string.IsNullOrEmpty(s),
                msg => $"{msg} isn't valid");

            vc.Add(v1);

            Assert.True(vc.IsValid);

            vm.Name = invalidName;

            Assert.False(v1.IsValid);
            Assert.False(vc.IsValid);

            Assert.Equal(1, vc.Text.Count);
        }

        /// <summary>
        /// Verifies that two validations properties are correctly applied in the <see cref="ValidationContext"/>.
        /// </summary>
        [Fact]
        public void TwoValidationComponentsCorrectlyResultInContextTest()
        {
            const string validName = "valid";
            var invalidName = string.Empty;

            var vc = new ValidationContext(ImmediateScheduler.Instance);

            var vm = new TestViewModel { Name = validName, Name2 = validName };

            var firstValidation = new BasePropertyValidation<TestViewModel, string>(
                vm,
                v => v.Name,
                s => !string.IsNullOrEmpty(s),
                s => $"Name {s} isn't valid");

            var secondValidation = new BasePropertyValidation<TestViewModel, string>(
                vm,
                v => v.Name2,
                s => !string.IsNullOrEmpty(s),
                s => $"Name 2 {s} isn't valid");

            vc.Add(firstValidation);
            vc.Add(secondValidation);

            Assert.True(vc.IsValid);
            Assert.Equal(0, vc.Text.Count);

            vm.Name = invalidName;
            Assert.False(vc.IsValid);
            Assert.Equal(1, vc.Text.Count);
            Assert.Equal("Name " + invalidName + " isn't valid", vc.Text[0]);

            vm.Name2 = invalidName;
            Assert.False(vc.IsValid);
            Assert.Equal(2, vc.Text.Count);
            Assert.Equal("Name " + invalidName + " isn't valid", vc.Text[0]);
            Assert.Equal("Name 2 " + invalidName + " isn't valid", vc.Text[1]);

            vm.Name = validName;
            vm.Name2 = validName;

            Assert.True(vc.IsValid);
            Assert.Equal(0, vc.Text.Count);
        }

        /// <summary>
        /// Verifies that two validations properties are correctly applied in a View property.
        /// </summary>
        [Fact]
        public void ShouldSupportBindingTwoValidationsForOneProperty()
        {
            const int minimumLength = 5;

            var viewModel = new TestViewModel { Name = "some" };
            var view = new TestView(viewModel);

            var firstValidation = new BasePropertyValidation<TestViewModel, string>(
                viewModel,
                vm => vm.Name,
                s => !string.IsNullOrEmpty(s),
                "Name is required.");

            var minimumLengthErrorMessage = $"Minimum length is {minimumLength}";
            var secondValidation = new BasePropertyValidation<TestViewModel, string>(
                viewModel,
                vm => vm.Name,
                s => s.Length > minimumLength,
                s => minimumLengthErrorMessage);

            // Add validations
            viewModel.ValidationContext.Add(firstValidation);
            viewModel.ValidationContext.Add(secondValidation);

            // View bindings
            view.Bind(view.ViewModel, vm => vm.Name, v => v.NameLabel);

            // View validations bindings
            view.BindValidation(view.ViewModel, vm => vm.Name, v => v.NameErrorLabel);

            viewModel.Name = "som";

            Assert.False(viewModel.ValidationContext.IsValid);
            Assert.Equal(2, viewModel.ValidationContext.Validations.Count);

            // Checks if second validation error message is shown
            Assert.Equal(minimumLengthErrorMessage, view.NameErrorLabel);
        }

        /// <summary>
        /// Verifies that two validations properties are correctly applied
        /// in a View property given by a complex expression.
        /// </summary>
        [Fact]
        public void ShouldSupportBindingTwoValidationsForOnePropertyToChainedViewProperties()
        {
            const int minimumLength = 5;
            var minimumLengthErrorMessage = $"Minimum length is {minimumLength}";
            var viewModel = new TestViewModel { Name = "some" };
            var view = new TestView(viewModel);

            // Define the validations.
            viewModel.ValidationRule(
                vm => vm.Name,
                s => !string.IsNullOrEmpty(s),
                "Name is required.");

            viewModel.ValidationRule(
                vm => vm.Name,
                s => s.Length > minimumLength,
                minimumLengthErrorMessage);

            // Define view bindings.
            view.Bind(view.ViewModel, vm => vm.Name, v => v.NameLabel);
            view.BindValidation(view.ViewModel, vm => vm.Name, v => v.NameErrorContainer.Text);

            Assert.False(viewModel.ValidationContext.IsValid);
            Assert.Equal(2, viewModel.ValidationContext.Validations.Count);
            Assert.Equal(minimumLengthErrorMessage, view.NameErrorContainer.Text);
        }

        /// <summary>
        /// Verifies that validations registered with different lambda names are retrieved successfully.
        /// </summary>
        [Fact]
        public void RegisterValidationsWithDifferentLambdaNameWorksTest()
        {
            const string validName = "valid";
            var viewModel = new TestViewModel { Name = validName };
            var view = new TestView(viewModel);

            var firstValidation = new BasePropertyValidation<TestViewModel, string>(
                viewModel,
                viewModelProperty => viewModelProperty.Name,
                s => !string.IsNullOrEmpty(s),
                s => $"Name {s} isn't valid");

            // Add validations
            viewModel.ValidationContext.Add(firstValidation);

            // View bindings
            view.Bind(view.ViewModel, vm => vm.Name, v => v.NameLabel);

            // This was throwing exception due to naming problems with lambdas expressions
            view.BindValidation(
                view.ViewModel,
                vm => vm.Name,
                v => v.NameErrorLabel);

            Assert.True(viewModel.ValidationContext.IsValid);
            Assert.Single(viewModel.ValidationContext.Validations);
        }

        /// <summary>
        /// Verifies that validation error messages get concatenated using white space.
        /// </summary>
        [Fact]
        public void ValidationMessagesDefaultConcatenationTest()
        {
            var viewModel = new TestViewModel { Name = string.Empty };
            var view = new TestView(viewModel);

            var nameValidation = new BasePropertyValidation<TestViewModel, string>(
                viewModel,
                viewModelProperty => viewModelProperty.Name,
                s => !string.IsNullOrEmpty(s),
                "Name should not be empty.");

            var name2Validation = new BasePropertyValidation<TestViewModel, string>(
                viewModel,
                viewModelProperty => viewModelProperty.Name2,
                s => !string.IsNullOrEmpty(s),
                "Name2 should not be empty.");

            viewModel.ValidationContext.Add(nameValidation);
            viewModel.ValidationContext.Add(name2Validation);

            view.Bind(view.ViewModel, vm => vm.Name, v => v.NameLabel);
            view.Bind(view.ViewModel, vm => vm.Name2, v => v.Name2Label);
            view.BindValidation(view.ViewModel, v => v.NameErrorLabel);

            Assert.False(viewModel.ValidationContext.IsValid);
            Assert.Equal(2, viewModel.ValidationContext.Validations.Count);
            Assert.NotEmpty(view.NameErrorLabel);
            Assert.Equal("Name should not be empty. Name2 should not be empty.", view.NameErrorLabel);
        }

        /// <summary>
        /// Verifies that this.IsValid() extension method observes a
        /// <see cref="ValidationContext"/> and emits new values.
        /// </summary>
        [Fact]
        public void IsValidShouldNotifyOfValidityChange()
        {
            var viewModel = new TestViewModel { Name = string.Empty };
            var nameValidation = new BasePropertyValidation<TestViewModel, string>(
                viewModel,
                viewModelProperty => viewModelProperty.Name,
                s => !string.IsNullOrEmpty(s),
                "Name should not be empty.");
            viewModel.ValidationContext.Add(nameValidation);

            var latestValidity = false;
            viewModel.IsValid().Subscribe(isValid => latestValidity = isValid);
            Assert.False(latestValidity);

            viewModel.Name = "Jonathan";
            Assert.True(latestValidity);

            viewModel.Name = string.Empty;
            Assert.False(latestValidity);
        }
    }
}
