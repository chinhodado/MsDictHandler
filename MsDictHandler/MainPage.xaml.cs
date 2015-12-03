using DictionaryManager;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            Loaded += PageLoaded;
        }

        private void HandleError(Operation operation, int err)
        {
            Debug.WriteLine("An error happened");
        }

        private void OperationCompleted(Operation operation)
        {
            Debug.WriteLine(operation.url());
        }

        private async void PageLoaded(object sender, RoutedEventArgs e)
        {
            InitializeDict();
            Uri myUrl = new Uri("http://www.google.ca");//myWebview.BuildLocalStreamUri("MyContent", "foobar"));
            StreamUriWinRTResolver myResolver = new StreamUriWinRTResolver();
            var x = await myResolver.UriToStreamAsync(myUrl);
            StreamReader stream = new StreamReader(x.AsStreamForRead());
            string html = stream.ReadToEnd().Trim();

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
            replacement = @"<span class='blue'>$1</span>";
            result = Regex.Replace(result, pattern, replacement);

            // blue bold underline
            pattern = @"<span style=""color:rgba\(54,95,145,1\);font-weight:bold;text-decoration: underline;"" >([^<>]+)</span>";
            replacement = @"<u class='blue bold'>$1</u>";
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
            replacement = @"<b class='blue'>$1</b>";
            result = Regex.Replace(result, pattern, replacement);

            // bold blue 1 span
            pattern = @"<span style=""color:rgba\(54,95,145,1\);font-weight:bold;"" >([^<>]+)</span>";
            replacement = @"<b class='blue'>$1</b>";
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
            result = Regex.Replace(result, @"style=""margin-bottom:20px;margin-top:0px;margin-left:0px;margin-right:0px;""", "class='section1'");
            result = Regex.Replace(result, @"style=""margin-bottom:4px;margin-top:0px;margin-left:4px;margin-right:0px;"" ", "class='section2'");
            result = Regex.Replace(result, @"style=""margin-bottom:10px;margin-top:0px;margin-left:0px;margin-right:0px;""", "class='section3'");
            result = Regex.Replace(result, @"style=""margin-bottom:0px;margin-top:4px;margin-left:0px;margin-right:0px;"" ", "class='section4'");
            result = Regex.Replace(result, @"style=""padding-bottom:0px;padding-top:0px;padding-left:2px;padding-right:0px;"" ", "class='section5'");
            result = Regex.Replace(result, @"style=""margin-bottom:8px;margin-top:0px;margin-left:0px;margin-right:0px;"" ", "class='section6'");
            result = Regex.Replace(result, @"style=""margin-bottom:8px;margin-top:0px;margin-left:0px;margin-right:0px;padding-bottom:0px;padding-top:0px;padding-left:2px;padding-right:0px;"" ", "class='section7'");
            result = Regex.Replace(result, @"style=""margin-bottom:4px;margin-top:0px;margin-left:0px;margin-right:0px;"" ", "class='section8'");
            result = Regex.Replace(result, @"style=""margin-bottom:0px;margin-top:10px;margin-left:0px;margin-right:0px;padding-bottom:2px;padding-top:2px;padding-left:2px;padding-right:2px; background-color:rgba\(240,240,240,1\);"" ", "class='section9'");

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
            replacement = @" class='blue'><span>";
            result = Regex.Replace(result, pattern, replacement, RegexOptions.Singleline);

            // can't remove completely since there are nested <span> inside
            result = Regex.Replace(result, @" style=""font-weight:bold;"" ", "");
            result = Regex.Replace(result, @" style="""" ", "");

            // outermost div
            pattern = @"<div style=""padding-bottom:2px;padding-top:2px;padding-left:2px;padding-right:2px;"" >(.*)</div>";
            result = Regex.Replace(result, pattern, "$1", RegexOptions.Singleline);

            // cambria 3 span
            pattern = @"<span style=""font-family:Cambria;"" ><span style=""font-family:Cambria;"" ><span style=""font-family:Cambria;"" >([^<>]+)</span></span></span>";
            replacement = @"<span class='cambria'>$1</span>";
            result = Regex.Replace(result, pattern, replacement);

            // cambria 2 span
            pattern = @"<span style=""font-family:Cambria;"" ><span style=""font-family:Cambria;"" >([^<>]+)</span></span>";
            replacement = @"<span class='cambria'>$1</span>";
            result = Regex.Replace(result, pattern, replacement);

            // cambria sup
            pattern = @"<span style=""font-family:Cambria;"" ><span style=""font-family:Cambria;"" ><span><span style=""vertical-align:super;"" >([^<>]+)</span></span></span></span>";
            replacement = @"<sup class='cambria'>$1</sup>";
            result = Regex.Replace(result, pattern, replacement);

            // underlined link
            pattern = @" style=""color:rgba\(54,95,145,1\);font-weight:bold;text-decoration: underline;"" id="""" ><u class='blue bold'>";
            replacement = @"><u class='blue bold'>";
            result = Regex.Replace(result, pattern, replacement);

            // blue bold that needs to be handled separately
            pattern = @"<span style=""color:rgba\(54,95,145,1\);font-weight:bold;"" ><span style=""color:rgba\(54,95,145,1\);font-weight:bold;"" >";
            replacement = @"<span><span class='blue bold'>";
            result = Regex.Replace(result, pattern, replacement);

            // stray brown
            result = Regex.Replace(result, @" style=""color:rgba\(99,36,35,1\);"" ", "");

            //Debug.WriteLine(html);
            result = result.Trim();
        }

        private async void button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //myWebview.NavigateToString(text);
            //myWebview.NavigateToLocalStreamUri(myUrl, myResolver);
        }

        private void InitializeDict()
        {
            Dictionary.AddDictionary(@"OxfordFrenchEnglish.dict");

            Operation op = Operation.crateLoadTOC(@"OxfordFrenchEnglish.dict");
            op.errorHandler += HandleError;
            op.post();
            Dictionary foo = Dictionary.currentDictionary();
            Debug.WriteLine(foo.phrase(123));
            op = Operation.crateLoadPhraseDefinitionOperation(foo.id(), "maison");
            op.operationCompletedHandler += OperationCompleted;
            op.post();
        }
    }
}
