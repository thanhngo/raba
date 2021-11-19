using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace RabaMetroStyle.CustomControl
{
    public class SelectedItemsBahavior
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached("SelectedItems", typeof(INotifyCollectionChanged), typeof(SelectedItemsBahavior),
            new PropertyMetadata(default(IList), OnSelectedItemsChanged));

        public static void SetSelectedItems(DependencyObject d, INotifyCollectionChanged value)
        {
            d.SetValue(SelectedItemsProperty, value);
        }

        public static IList GetSelectedItems(DependencyObject d)
        {
            return (IList)d.GetValue(SelectedItemsProperty);
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IList selectedItems = null;

            void CollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs args)
            {
                if (args.OldItems != null)
                    foreach (var item in args.OldItems)
                        if (selectedItems.Contains(item))
                            selectedItems.Remove(item);

                if (args.NewItems != null)
                    foreach (var item in args.NewItems)
                        if (!selectedItems.Contains(item))
                            selectedItems.Add(item);
            }

            if (d is MultiSelector multiSelector)
            {
                selectedItems = multiSelector.SelectedItems;
                multiSelector.SelectionChanged += OnSelectionChanged;
            }
            if (d is ListBox listBox)
            {
                selectedItems = listBox.SelectedItems;
                listBox.SelectionMode = SelectionMode.Multiple;
                listBox.SelectionChanged += OnSelectionChanged;
            }
            if (selectedItems == null) return;

            if (e.OldValue is INotifyCollectionChanged collection1)
                collection1.CollectionChanged -= CollectionChangedEventHandler;
            if (e.NewValue is INotifyCollectionChanged collection2)
                collection2.CollectionChanged += CollectionChangedEventHandler;
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var s = sender as DependencyObject;
            if (!GetIsBusy(s))
            {
                SetIsBusy(s, true);
                var list = GetSelectedItems((DependencyObject)sender);
                foreach (var item in e.RemovedItems)
                    if (list.Contains(item)) list.Remove(item);
                foreach (var item in e.AddedItems)
                    if (!list.Contains(item)) list.Add(item);
                SetIsBusy(s, false);
            }
        }


        private static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.RegisterAttached("IsBusy", typeof(bool), typeof(SelectedItemsBahavior), new PropertyMetadata(default(bool)));

        private static void SetIsBusy(DependencyObject element, bool value)
        {
            element.SetValue(IsBusyProperty, value);
        }

        private static bool GetIsBusy(DependencyObject element)
        {
            return (bool)element.GetValue(IsBusyProperty);
        }
    }
}
