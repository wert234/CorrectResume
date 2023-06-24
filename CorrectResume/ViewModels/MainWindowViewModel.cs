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
    public class MainWindowViewModel : ViewModels.Base.ViewModel
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

        private ObservableCollection<string> _Responsibilities = new ObservableCollection<string>();
		public ObservableCollection<string> Responsibilities
        {
			get => _Responsibilities;
			set
            {
                if(Equals(Responsibilities, value)) return;
                _Responsibilities = value;
                OnPropertyCanged();
            }
		}

        private ObservableCollection<string> _Conditions = new ObservableCollection<string>();
        public ObservableCollection<string> Conditions
        {
            get => _Conditions;
            set
            {
                if(Equals(Conditions, value)) return;
                _Conditions = value;
                OnPropertyCanged();
            }
        }

        private ObservableCollection<string> _Requirement = new ObservableCollection<string>();
        public ObservableCollection<string> Requirement
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
        private async void OnSaveResume(object p)
        {
            var resume = new SaveFileDialog();
            resume.ShowDialog();
            if (resume.FileName != "")
            {
                using (var stream = resume.OpenFile())
                {
                    string responsibilities = "";
                    foreach (var item in _Responsibilities)
                        responsibilities = responsibilities + "\n" + item;

                    string conditions = "";
                    foreach (var item in _Conditions)
                        conditions = conditions + "\n" + item;

                    string requirement = "";
                    foreach (var item in _Requirement)
                        requirement = requirement + "\n" + item;

                    var saveDate = $"\n Должностные обязанности:\n\n{responsibilities}\n\n Условия:{conditions} \n\n Требование к соискателю:{requirement}\n";

                    var writer = new StreamWriter(stream);
                   await writer.WriteAsync(saveDate);
                    writer.Flush();

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
                     Responsibilities.Add(line);
                else if(predict == 1)
                    Conditions.Add(line);
                else if (predict == 2)
                    Requirement.Add(line);
            }
        }
        #endregion

        #region Init

        private TextModel _Model;
        private ITransformer _Transformer;
        private List<string> _Texts = new List<string>();
        private int _TextIndex = 0;

        public MainWindowViewModel()
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
