using CorrectResume.Infrastructure.Commands.Base;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using NeuroLib.Models;
using NeuroLib.Models.TextModel;
using Microsoft.ML;
using System.Windows.Shapes;
using Extension.OfficeOpenXml;
using Aspose.Cells;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.ExtendedProperties;
using System.IO;

namespace CorrectResume.ViewModels
{
    internal class MainWindowViewModel : ViewModels.Base.ViewModel
    {
        #region Property

        private string _Resume = "";

        public string Resume
        {
            get => _Resume; 
            set
            {
                if(Equals(_Resume, value)) return;
                _Resume = value;
                OnPropertyCanged();
            }
        }

        private ObservableCollection<TextBlock> _Responsibilities = new ObservableCollection<TextBlock>();
		public ObservableCollection<TextBlock> Responsibilities
        {
			get => _Responsibilities;
			set
            {
                if(Equals(Responsibilities, value)) return;
                _Responsibilities = value;
                OnPropertyCanged();
            }
		}

        private ObservableCollection<TextBlock> _Conditions = new ObservableCollection<TextBlock>();
        public ObservableCollection<TextBlock> Conditions
        {
            get => _Conditions;
            set
            {
                if(Equals(Conditions, value)) return;
                _Conditions = value;
                OnPropertyCanged();
            }
        }

        private ObservableCollection<TextBlock> _Requirement = new ObservableCollection<TextBlock>();
        public ObservableCollection<TextBlock> Requirement
        {
            get => _Requirement;
            set
            {
                if(Equals(Requirement, value)) return;
                _Requirement = value;
                OnPropertyCanged();
            }
        }
        #endregion

        #region Commands

        public ICommand SwipeRight { get; set; }

        private bool CanSwipeRight(object p) => true;
        private void OnSwipeRight(object p)
        {
            Resume = _Texts[_TextIndex++];
        }

        public ICommand SwipeLeft { get; set; }

        private bool CanSwipeLeft(object p) => true;
        private void OnSwipeLeft(object p)
        {
            Resume = _Texts[_TextIndex--];
        }

        public ICommand OpenResume { get; set; }

        private bool CanOpenResume(object p) => true;
        private void OnOpenResume(object p)
        {
            var resume = new OpenFileDialog();
            resume.ShowDialog();
            if(resume.FileName != "")
            {     
                using (var stream = resume.OpenFile())
                {
                    Workbook workbook = new Workbook(resume.FileName);
                    Worksheet worksheet = workbook.Worksheets[0];

                    for (int i = 2; i < worksheet.Cells.MaxDataRow; i++)
                        _Texts.Add(worksheet.Cells[$"A{i}"].StringValue);
                    Resume = _Texts[_TextIndex];
                };
            }
        }

        public ICommand SaveResume { get; set; }

        private bool CanSaveResume(object p) => true;
        private void OnSaveResume(object p)
        {
         //   _Model.CreatModel(@"C:\Users\Vladlen\Desktop\Data.txt", "NeuroLibModel2.zip");

            var resume = new SaveFileDialog();
            resume.ShowDialog();
            if (resume.FileName != "")
            {
                using (var stream = resume.OpenFile())
                {
                    //Обработка данных
                };
            }
        }

        public ICommand ConvertResume { get; set; }

        private bool CanConvertResume(object p) => true;
        private void OnConvertResume(object p)
        {
            var text = Regex.Replace(Resume, @"[А-Я][а-я]+(\:| \:)", "").Split(new string[] { ". ", ";", " - ", "!", "•", "·", " — " }, StringSplitOptions.RemoveEmptyEntries);
             
            Responsibilities.Clear();
            Conditions.Clear();
            Requirement.Clear();

            List<string> filteredLines = new List<string>();
            foreach (string line in text)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    filteredLines.Add(line);
                }
            }

           var text2 = filteredLines.Where(x => x.Split(new[] {' '}).Length > 2).Select(x => x.Replace("\n", ""));

            foreach (var line in text2)
            {
                var predict = _Model.Predict(_Transformer, new ModelInput() { Sentence = line });

                if (predict == 0)
                     Responsibilities.Add(new TextBlock(new Run(line)));
                else if(predict == 1)
                    Conditions.Add(new TextBlock(new Run(line)));
                else if (predict == 2)
                    Requirement.Add(new TextBlock(new Run(line)));
            }
        }
        #endregion

        #region Init

        private TextModel _Model;
        private ITransformer _Transformer;
        private List<string> _Texts = new List<string>();
        private int _TextIndex = 0;

        internal MainWindowViewModel()
        {
            _Model = new TextModel();
            _Transformer = _Model.LoadModel("NeuroLibModel2.zip");


           OpenResume = new CommandIniter(OnOpenResume, CanOpenResume);
           SaveResume = new CommandIniter(OnSaveResume, CanSaveResume);
           ConvertResume = new CommandIniter(OnConvertResume, CanConvertResume);
           SwipeLeft = new CommandIniter(OnSwipeLeft, CanSwipeLeft);
           SwipeRight = new CommandIniter(OnSwipeRight, CanSwipeRight);
        }
        #endregion
    }
}
