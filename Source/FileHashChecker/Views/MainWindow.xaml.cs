﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using FileHashChecker.ViewModels;
using FileHashChecker.Views.Converters;

namespace FileHashChecker.Views
{
	public partial class MainWindow : Window
	{
		#region Property

		public bool IsMenuOpen
		{
			get { return (bool)GetValue(IsMenuOpenProperty); }
			set { SetValue(IsMenuOpenProperty, value); }
		}
		public static readonly DependencyProperty IsMenuOpenProperty =
			DependencyProperty.Register(
				"IsMenuOpen",
				typeof(bool),
				typeof(MainWindow),
				new PropertyMetadata(false));

		#endregion

		private readonly MainWindowViewModel _mainWindowViewModel;

		public MainWindow()
		{
			InitializeComponent();

			_mainWindowViewModel = (MainWindowViewModel)this.DataContext;

			this.SetBinding(
				AllowDropProperty,
				new Binding(nameof(MainWindowViewModel.IsReading))
				{
					Source = _mainWindowViewModel,
					Mode = BindingMode.OneWay,
					Converter = new BooleanInverseConverter()
				});

			this.Loaded += OnLoaded;
			this.Drop += OnDrop;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.TitleBar.MouseDown += (_, e) =>
			{
				e.Handled = true;

				if (IsMenuOpen)
					IsMenuOpen = false;
				else
					this.DragMove();
			};
			this.MenuPain.MouseDown += (_, e) => { e.Handled = true; };
			this.MouseDown += (_, e) => { IsMenuOpen = false; };
		}

		protected override void OnStateChanged(EventArgs e)
		{
			// Prevent Aero Snap.
			if (WindowState == WindowState.Maximized)
				WindowState = WindowState.Normal;

			base.OnStateChanged(e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			_mainWindowViewModel.Cancel();
		}

		private async void OnLoaded(object sender, RoutedEventArgs e)
		{
			var args = Environment.GetCommandLineArgs().Skip(1); // The first element is this executable file path.

			await _mainWindowViewModel.CheckFileAsync(args);
		}

		private async void OnDrop(object sender, DragEventArgs e)
		{
			var paths = ((DataObject)e.Data).GetFileDropList().Cast<string>();

			await _mainWindowViewModel.CheckFileAsync(paths);
		}

		private void Menu_Click(object sender, RoutedEventArgs e)
		{
			IsMenuOpen = !IsMenuOpen;
		}

		private void Minimize_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}

		private void Close_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private async void Browse_Click(object sender, RoutedEventArgs e)
		{
			await _mainWindowViewModel.SelectFileAsync();
		}

		private void Clipboard_Click(object sender, RoutedEventArgs e)
		{
			_mainWindowViewModel.ReadClipboard();
		}

		private async void Compute_Click(object sender, RoutedEventArgs e)
		{
			await _mainWindowViewModel.ComputeHashValuesAsync();
		}

		private void Stop_Click(object sender, RoutedEventArgs e)
		{
			_mainWindowViewModel.Cancel();
		}
	}
}