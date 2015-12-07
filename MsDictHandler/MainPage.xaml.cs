using DictionaryManager;
using SQLite;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MsDictHandler {
    /// <summary>
    /// Build a SQLite database from a MSDict .dict database
    /// (only tested on the Oxford Hachette French Dictionary
    /// Every time this app loads, it looks for the file test.db, checks the Word table,
    /// and inserts in there the next 500 words, then exits itself.
    /// </summary>
    public sealed partial class MainPage : Page {
        private SQLiteConnection db;
        private Dictionary dict;
        string[] wordList = new string[40854];
        StreamUriWinRTResolver myResolver = new StreamUriWinRTResolver();
        Uri myUrl = new Uri("http://www.google.ca");
        private string dictName = "OxfordFrenchEnglish.dict";
        private uint phraseCount;

        public MainPage() {
            InitializeComponent();
            Loaded += PageLoaded;
        }

        private void HandleError(Operation operation, int err) {
            Debug.WriteLine("An error happened");
        }

        private async void OperationCompleted(Operation operation) {
            string html;
            // there is a massive leak inside UriToStreamAsync(). Every time you call it,
            // it creates a handle for the dictionary file, but never releases the handle.
            // Since the maximum number of handle (per process?) possible is 512, after about
            // 512 words have been read (may not be exact 512, since there can be stdio and such),
            // the word definition will be repeating. Of course, I can't do anything about it.
            // Thus, the app needs to be restarted every 500 words.
            using (IInputStream x = await myResolver.UriToStreamAsync(myUrl)) {
                using (StreamReader stream = new StreamReader(x.AsStreamForRead())) {
                    html = stream.ReadToEnd().Trim();
                }
            }

            string cleanHtml = GetProcessedHtml(html);
            var url = operation.url();
            int id = int.Parse(url.Substring(url.IndexOf(@"=", StringComparison.Ordinal) + 1));

            db.Insert(new Word {
                name = wordList[id],
                definition = cleanHtml
            });
            operation.Dispose();

            if (id % 500 == 499 || id >= phraseCount) {
                Application.Current.Exit();
            }

            // load the next word definition, in a recursive way. But of course it's not real recursion
            // since this function/event handler can return immediately after posting the new operation
            Operation op = Operation.crateLoadPhraseDefinitionOperation(dictName, (uint)id + 1);
            op.operationCompletedHandler += OperationCompleted;
            op.post();
        }

        private void PageLoaded(object sender, RoutedEventArgs e) {
            // initialize dict
            Dictionary.AddDictionary(dictName);
            Operation op = Operation.crateLoadTOC(dictName);
            op.errorHandler += HandleError;
            op.post();

            dict = Dictionary.currentDictionary();
            phraseCount = dict.phraseCount();

            // initialize db
            string path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "test.db");
            db = new SQLiteConnection(path, true);
            db.CreateTable<Word>();
            int count = db.ExecuteScalar<int>("select count(*) from Word");

            if (count == 0) {
                db.Query<Word>("CREATE INDEX name_idx ON Word (name)");
            }

            if (count == phraseCount) {
                textBox.Text = "All done!";
                return;
            }

            for (uint i = 0; i < 40854; i++) {
                wordList[i] = dict.phrase(i);
            }

            op = Operation.crateLoadPhraseDefinitionOperation(dictName, (uint)count);
            op.operationCompletedHandler += OperationCompleted;
            op.post();
        }

        /// <summary>
        /// Get the cleaned up HTML from the initial verbose HTML.
        /// </summary>
        /// <param name="html">Source HTML</param>
        /// <returns>The cleaned up HTML</returns>
        private string GetProcessedHtml(string html) {
            // useless attributes
            string result = Regex.Replace(html, @"-ms-text-size-adjust:none;", "");
            result = Regex.Replace(result, @"color:rgba\(0,0,0,1\);", "");
            result = Regex.Replace(result, @"font-size:21\.3px;", "");
            result = Regex.Replace(result, @"font-style:normal;", "");
            result = Regex.Replace(result, @"class=""(EN|FR)?"" ", "");

            // bold
            string pattern =
                @"<span style=""font-weight:bold;"" ><span style=""font-weight:bold;"" >([^<>]+)</span></span>";
            string replacement = "<b>$1</b>";
            result = Regex.Replace(result, pattern, replacement);

            // normal
            pattern =
                @"<span style="""" ><span style="""" >([^<>]+)</span></span>";
            replacement = "$1";
            result = Regex.Replace(result, pattern, replacement);

            // hidden br
            result = Regex.Replace(result, @"<br class=""hidden"" />", "");

            // green
            pattern = @"<span style=""color:rgba\(118,156,60,1\);"" ><span style=""color:rgba\(118,156,60,1\);"" >([^<>]+)</span></span>";
            replacement = @"<span class='green'>$1</span>";
            result = Regex.Replace(result, pattern, replacement);

            // italic
            pattern = @"<span style=""font-style:italic;"" ><span style=""font-style:italic;"" >([^<>]+)</span></span>";
            replacement = @"<i>$1</i>";
            result = Regex.Replace(result, pattern, replacement);

            // empty span (single space)
            pattern = @"<span style="""" > </span>";
            result = Regex.Replace(result, pattern, " ");

            // blue
            pattern = @"<span style=""color:rgba\(54,95,145,1\);"" ><span style=""color:rgba\(54,95,145,1\);"" >([^<>]+)</span></span>";
            replacement = @"<span class='bl'>$1</span>";
            result = Regex.Replace(result, pattern, replacement);

            // blue bold underline
            pattern = @"<span style=""color:rgba\(54,95,145,1\);font-weight:bold;text-decoration: underline;"" >([^<>]+)</span>";
            replacement = @"<u class='bl b'>$1</u>";
            result = Regex.Replace(result, pattern, replacement);

            // red
            pattern = @"<span style=""color:rgba\(192,80,77,1\);"" ><span style=""color:rgba\(192,80,77,1\);"" >([^<>]+)</span></span>";
            replacement = @"<span class='red'>$1</span>";
            result = Regex.Replace(result, pattern, replacement);

            // red italic
            pattern = @"<span style=""color:rgba\(99,36,35,1\);font-style:italic;"" ><span style=""color:rgba\(99,36,35,1\);font-style:italic;"" >([^<>]+)</span></span>";
            replacement = @"<i class='red'>$1</i>";
            result = Regex.Replace(result, pattern, replacement);

            // bold blue
            pattern = @"<span style=""color:rgba\(54,95,145,1\);font-weight:bold;"" ><span style=""color:rgba\(54,95,145,1\);font-weight:bold;"" >([^<>]+)</span></span>";
            replacement = @"<b class='bl'>$1</b>";
            result = Regex.Replace(result, pattern, replacement);

            // bold blue 1 span
            pattern = @"<span style=""color:rgba\(54,95,145,1\);font-weight:bold;"" >([^<>]+)</span>";
            replacement = @"<b class='bl'>$1</b>";
            result = Regex.Replace(result, pattern, replacement);

            // orange bold
            pattern = @"<span style=""color:rgba\(247,150,70,1\);font-weight:bold;"" >([^<>]+)</span>";
            replacement = @"<b class='orange'>$1</b>";
            result = Regex.Replace(result, pattern, replacement);

            // brown
            pattern = @"<span style=""color:rgba\(99,36,35,1\);"" >([^<>]+)</span>";
            replacement = @"<span class='brown'>$1</span>";
            result = Regex.Replace(result, pattern, replacement);

            // purple italic
            pattern = @"<span style=""color:rgba\(95,73,122,1\);font-style:italic;"" ><span style=""color:rgba\(95,73,122,1\);font-style:italic;"" >([^<>]+)</span></span>";
            replacement = @"<i class='purple'>$1</i>";
            result = Regex.Replace(result, pattern, replacement);

            // normal 1 span
            pattern = @"<span style="""" >([^<>]+)</span>";
            replacement = @"$1";
            result = Regex.Replace(result, pattern, replacement);

            // bold 1 span
            pattern = @"<span style=""font-weight:bold;"" >([^<>]+)</span>";
            replacement = @"<b>$1</b>";
            result = Regex.Replace(result, pattern, replacement);

            // title. God save me...
            pattern = @"<span style="""" ><span style=""color:rgba\(54,95,145,1\);font-weight:bold;font-size:24px;"" ><span style=""color:rgba\(54,95,145,1\);font-weight:bold;font-size:24px;"" >([^<>]+)</span></span></span>";
            replacement = @"<span class='title'>$1</span>";
            result = Regex.Replace(result, pattern, replacement);

            // title with superscript
            pattern = @"<span style=""color:rgba\(54,95,145,1\);font-weight:bold;font-size:24px;"" ><span style=""color:rgba\(54,95,145,1\);font-weight:bold;font-size:24px;"" >([^<>]+)</span></span>";
            replacement = @"<span class='title'>$1</span>";
            result = Regex.Replace(result, pattern, replacement);

            // title's superscript
            pattern = @"<span style=""color:rgba\(54,95,145,1\);font-weight:bold;font-size:24px;"" ><span style=""color:rgba\(54,95,145,1\);font-weight:bold;"" ><span style=""color:rgba\(54,95,145,1\);font-weight:bold;vertical-align:super;"" >([^<>]+)</span></span></span>";
            replacement = @"<sup class='title'>$1</sup>";
            result = Regex.Replace(result, pattern, replacement);

            // title's superscript ver 2
            pattern = @"<span style=""color:rgba\(54,95,145,1\);font-weight:bold;font-size:24px;"" ><span style=""color:rgba\(54,95,145,1\);font-weight:bold;font-size:24px;"" ><span style=""color:rgba\(54,95,145,1\);font-weight:bold;font-size:24px;vertical-align:super;"" >([^<>]+)</span></span></span>";
            replacement = @"<sup class='title'>$1</sup>";
            result = Regex.Replace(result, pattern, replacement);

            // section div
            result = Regex.Replace(result, @"style=""margin-bottom:20px;margin-top:0px;margin-left:0px;margin-right:0px;""", "class='s1'");
            result = Regex.Replace(result, @"style=""margin-bottom:4px;margin-top:0px;margin-left:4px;margin-right:0px;"" ", "class='s2'");
            result = Regex.Replace(result, @"style=""margin-bottom:10px;margin-top:0px;margin-left:0px;margin-right:0px;""", "class='s3'");
            result = Regex.Replace(result, @"style=""margin-bottom:0px;margin-top:4px;margin-left:0px;margin-right:0px;"" ", "class='s4'");
            result = Regex.Replace(result, @"style=""padding-bottom:0px;padding-top:0px;padding-left:2px;padding-right:0px;"" ", "class='s5'");
            result = Regex.Replace(result, @"style=""margin-bottom:8px;margin-top:0px;margin-left:0px;margin-right:0px;"" ", "class='s6'");
            result = Regex.Replace(result, @"style=""margin-bottom:8px;margin-top:0px;margin-left:0px;margin-right:0px;padding-bottom:0px;padding-top:0px;padding-left:2px;padding-right:0px;"" ", "class='s7'");
            result = Regex.Replace(result, @"style=""margin-bottom:4px;margin-top:0px;margin-left:0px;margin-right:0px;"" ", "class='s8'");
            result = Regex.Replace(result, @"style=""margin-bottom:0px;margin-top:10px;margin-left:0px;margin-right:0px;padding-bottom:2px;padding-top:2px;padding-left:2px;padding-right:2px; background-color:rgba\(240,240,240,1\);"" ", "class='s9'");
            result = Regex.Replace(result, @"style=""padding-bottom:2px;padding-top:2px;padding-left:2px;padding-right:2px; background-color:rgba\(238,236,225,1\);"" ", "class='s10'");
            result = Regex.Replace(result, @"style=""margin-bottom:4px;margin-top:10px;margin-left:0px;margin-right:0px;"" ", "class='s11'");

            // empty span
            pattern = @"<span style=""color:rgba\(54,95,145,1\);font-weight:bold;"" ></span>";
            result = Regex.Replace(result, pattern, "");

            // <head>, greedy intentionally
            pattern = @"<head>(.*)</head>";
            result = Regex.Replace(result, pattern, "", RegexOptions.Singleline);

            // <html>
            pattern = @"<html >(.*)</html>";
            result = Regex.Replace(result, pattern, "$1", RegexOptions.Singleline);

            // <meta>
            result = Regex.Replace(result, @"<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />", "");

            // <script>
            result = Regex.Replace(result, @"<script type=""text/javascript"" src=""local_scripts.js"" ></script>", "");

            // superscript o
            pattern = @"<span style=""font-family:Cambria;"" ><span style=""font-family:Cambria;"" ><span style=""font-family:Cambria;vertical-align:super;"" >([^<>]+)</span></span></span>";
            result = Regex.Replace(result, pattern, "<sup>$1</sup>");

            // divs to br
            // TODO: careful. this will only work if there's no nested div no style
            pattern = @"<div style="""" >(.*?)</div>";
            replacement = @"<br>$1<br>";
            result = Regex.Replace(result, pattern, replacement, RegexOptions.Singleline);

            // reduce the amount of <br> from the previous step
            result = Regex.Replace(result, "<br><br>", "<br>", RegexOptions.Singleline);

            pattern = @" style=""color:rgba\(54,95,145,1\);"" id="""" ><span style=""color:rgba\(54,95,145,1\);"" >";
            replacement = @" class='bl'><span>";
            result = Regex.Replace(result, pattern, replacement, RegexOptions.Singleline);

            // can't remove completely since there are nested <span> inside
            result = Regex.Replace(result, @" style=""font-weight:bold;"" ", "");
            result = Regex.Replace(result, @" style="""" ", "");

            // outermost div
            pattern = @"<div style=""padding-bottom:2px;padding-top:2px;padding-left:2px;padding-right:2px;"" >(.*)</div>";
            result = Regex.Replace(result, pattern, "$1", RegexOptions.Singleline);

            // cambria 3 span
            pattern = @"<span style=""font-family:Cambria;"" ><span style=""font-family:Cambria;"" ><span style=""font-family:Cambria;"" >([^<>]+)</span></span></span>";
            replacement = @"<span class='sr'>$1</span>";
            result = Regex.Replace(result, pattern, replacement);

            // cambria 2 span
            pattern = @"<span style=""font-family:Cambria;"" ><span style=""font-family:Cambria;"" >([^<>]+)</span></span>";
            replacement = @"<span class='sr'>$1</span>";
            result = Regex.Replace(result, pattern, replacement);

            // cambria sup
            pattern = @"<span style=""font-family:Cambria;"" ><span style=""font-family:Cambria;"" ><span><span style=""vertical-align:super;"" >([^<>]+)</span></span></span></span>";
            replacement = @"<sup class='sr'>$1</sup>";
            result = Regex.Replace(result, pattern, replacement);

            // cambria 2 span sup
            pattern = @"<span style=""font-family:Cambria;"" ><span style=""font-family:Cambria;vertical-align:super;"" >([^<>]+)</span></span>";
            replacement = @"<sup class='sr'>$1</sup>";
            result = Regex.Replace(result, pattern, replacement);

            // space
            result = Regex.Replace(result, @"<span style=""font-family:Cambria;"" > </span>", " ");

            // underlined link
            pattern = @" style=""color:rgba\(54,95,145,1\);font-weight:bold;text-decoration: underline;"" id="""" ><u class='blue bold'>";
            replacement = @"><u class='bl b'>";
            result = Regex.Replace(result, pattern, replacement);

            // blue bold that needs to be handled separately
            pattern = @"<span style=""color:rgba\(54,95,145,1\);font-weight:bold;"" ><span style=""color:rgba\(54,95,145,1\);font-weight:bold;"" >";
            replacement = @"<span><span class='bl b'>";
            result = Regex.Replace(result, pattern, replacement);

            // title with nested cambria
            pattern = @"<span style=""color:rgba\(54,95,145,1\);font-weight:bold;font-size:24px;"" ><span style=""color:rgba\(54,95,145,1\);font-weight:bold;font-size:24px;"" >([^<>]+)</span>";
            replacement = @"<span class='title'>$1";
            result = Regex.Replace(result, pattern, replacement);

            // empty href
            result = Regex.Replace(result, @"href=""""", "");

            // empty id
            result = Regex.Replace(result, @"id=""""", "");

            // stray blue
            result = Regex.Replace(result, @"color:rgba\(54,95,145,1\);", "");

            // stray brown
            result = Regex.Replace(result, @"color:rgba\(99,36,35,1\);", "");

            // stray bold
            result = Regex.Replace(result, @"font-weight:bold;", "");

            // stray underline
            result = Regex.Replace(result, @"text-decoration: underline;", "");

            // stray title font size
            result = Regex.Replace(result, @"font-size:24px;", "");

            // empty style
            result = Regex.Replace(result, @"style=""""", "");

            result = result.Trim();
            return result;
        }
    }
}
