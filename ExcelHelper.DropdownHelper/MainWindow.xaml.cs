﻿using System.Windows.Input;
using Xl = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ExcelHelper.DropdownHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Xl.Application activeApp;
        private Xl.Workbook activeWb;
        private Xl.Worksheet activeWs;
        private Xl.Range activeRange;
        public List<string> validationList;
        public MainWindow(Xl.Application excelApp)
        {
            InitializeComponent();
            activeApp = excelApp;
            RefreshActive();
            activeWs.SelectionChange += new Microsoft.Office.Interop.Excel.DocEvents_SelectionChangeEventHandler(SelectionChange);
            validationList = ReadDropDownValues(activeWb, activeRange);
            SearchBox.ItemsSource = validationList;
        }
        void SelectionChange(Xl.Range Target)
        {
            RefreshActive();
            validationList = ReadDropDownValues(activeWb, activeRange);
            this.Dispatcher.Invoke(() =>
            {
                SearchBox.ItemsSource = validationList;
            });
        }
        void WindowActivated(object sender, EventArgs e)
        {
            RefreshActive();
            activeWs.SelectionChange += new Microsoft.Office.Interop.Excel.DocEvents_SelectionChangeEventHandler(SelectionChange);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this.SearchBox);
        }
        private void btnFill(object sender, RoutedEventArgs e)
        {
            RefreshActive();
            activeRange.Value2 = SearchBox.Text;
        }
        private void RefreshActive()
        {
            activeWb = activeApp.ActiveWorkbook;
            activeWs = activeWb.ActiveSheet;
            activeRange = activeApp.Selection;
        }
        List<string> ReadDropDownValues(Xl.Workbook xlWorkBook, Xl.Range dropDownCell)
        {
            List<string> result = new List<string>();
            string formulaRange;
            //Test if cell has validation
            try
            {
                formulaRange = dropDownCell.Validation.Formula1;
                //Test if the validation is a list, a formula reference, or a named range reference
                Xl.Worksheet xlWorkSheet;
                string[] splitFormulaRange;
                Xl.Range valRange;
                if (formulaRange.Contains(","))
                {
                    result = formulaRange.Split(',').ToList();
                }
                else
                {
                    if (formulaRange.Contains(":"))
                    {
                        //test if there is external reference
                        if (formulaRange.Contains("!"))
                        {
                            string[] formulaRangeWorkSheetAndCells = formulaRange.Substring(1, formulaRange.Length - 1).Split('!');
                            splitFormulaRange = formulaRangeWorkSheetAndCells[1].Split(':');
                            xlWorkSheet = xlWorkBook.Worksheets.get_Item(formulaRangeWorkSheetAndCells[0]);
                        }
                        else
                        {

                            splitFormulaRange = formulaRange.Substring(1, formulaRange.Length - 1).Split(':');
                            xlWorkSheet = activeWs;
                        }
                        valRange = xlWorkSheet.get_Range(splitFormulaRange[0], splitFormulaRange[1]);
                    }
                    else
                    {
                        if (formulaRange.Contains("!"))
                        {
                            string[] formulaRangeWorkSheetAndCells = formulaRange.Substring(1, formulaRange.Length - 1).Split('!');
                            xlWorkSheet = xlWorkBook.Worksheets.get_Item(formulaRangeWorkSheetAndCells[0]);
                            valRange = xlWorkSheet.get_Range(formulaRangeWorkSheetAndCells[1]);
                        }
                        else
                        {
                            valRange = activeApp.get_Range(formulaRange.Substring(1, formulaRange.Length - 1));
                        }
                    }
                    for (int nRows = 1; nRows <= valRange.Rows.Count; nRows++)
                    {
                        for (int nCols = 1; nCols <= valRange.Columns.Count; nCols++)
                        {
                            Xl.Range aCell = (Xl.Range)valRange.Cells[nRows, nCols];
                            if (aCell.Value2 != null)
                            {
                                result.Add(aCell.Value2.ToString());
                            }
                        }
                    }
                }
            }
            catch (COMException e)
            {
            }
            return result;
        }
    }
}
