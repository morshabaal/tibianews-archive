//--------------------------------------------------------------------------------
//
// TibiaNews.net archiving tool. This tool archives old TibiaNews.net articles.
//
// Created by cyph3rjo3 -a.k.a.- morshabaal
// May 2024
//
// https://github.com/morshabaal/tibianews-archive
//
// DISCLAIMER: The code in this file is messy and was quickly put together just to
// get the job done. It archives all the TibiaNews.net articles, both in the old
// format and the new format. All articles are saved as JSON files.
//
// INFORMATION: This software is licensed under GNU GENERAL PUBLIC LICENSE v3.0
// See the LICENSE file for more information.
//
//--------------------------------------------------------------------------------
using System;
using System.IO;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Net.Http;
using System.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TibiaNewsArchive
{
    public class Program
    {
        // List of links
        public static List<string> links = new List<string>();

        public static void Main(string[] args)
        {
            //--------------------------------------------------------------------------------
            //
            // Old links (before TibiaNews.net redesigned their website around 2010)
            //
            //--------------------------------------------------------------------------------

            // Load all old links and sort them
            links.AddRange(File.ReadAllLines("old-links.txt"));
            links.Sort();

            // Remove duplicate links (if any)
            List<string> sorted = links.Distinct().ToList();

            // Iterate through each old link
            foreach (string link in sorted)
            {
                try
                {
                    // Scrape each link
                    var html = link;
                    HtmlWeb web = new HtmlWeb();
                    var htmlDoc = web.Load(html);

                    // Get the article ID
                    int id = Int32.Parse(Regex.Match(link, @"(?:\?|&)id=(\d+)").Groups[1].Value);

                    // Get the date
                    string dateString = htmlDoc.DocumentNode.SelectSingleNode(".//font[@color='#FFCC00']").InnerText.Replace("&nbsp;", "");
                    DateTime dateTime = DateTime.ParseExact(dateString, "dd/MM/yyyy", null);
                    string date = dateTime.ToString("yyyy-MM-dd HH:mm:ss");

                    // Get the title
                    string title = htmlDoc.DocumentNode.SelectSingleNode(".//font[@color='#990000']").InnerText.Replace("<br>", "").Replace("&quot;", "");
                    title = Regex.Replace(title, @"\r\n?|\n|\t", "");
                    title = Regex.Replace(title, @"[^\x20-\x7E]", "");

                    // Get the author
                    string author = htmlDoc.DocumentNode.SelectSingleNode("(.//font[@size='1'])[2]").InnerText.Replace("<br>", "").Replace("&quot;", "");
                    author = Regex.Replace(author, @"\r\n?|\n|\t", "");
                    author = Regex.Replace(author, @"[^\x20-\x7E]", "");

                    // Get the content
                    string content = Regex.Replace(htmlDoc.DocumentNode.SelectSingleNode(".//font[@size='2']").InnerHtml, @"[^\x20-\x7E]", "");

                    // New Article
                    Article article = new Article();
                    article.id = id;
                    article.date = date;
                    article.title = title;
                    article.author = author;
                    article.content = content;

                    // JSON serialize the article
                    string json = JsonSerializer.Serialize(article);

                    // Save article as JSON file
                    long timeNowInSeconds = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
                    string filePath = Path.Combine(Environment.CurrentDirectory, "archive", id + timeNowInSeconds + ".json");
                    if (File.Exists(filePath))
                    {
                        timeNowInSeconds = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
                        File.WriteAllText(string.Concat(Environment.CurrentDirectory, @"\archive\" + id + timeNowInSeconds + ".json"), json);
                    }
                    else
                    {
                        File.WriteAllText(string.Concat(Environment.CurrentDirectory, @"\archive\" + id + timeNowInSeconds + ".json"), json);
                    }

                    Console.WriteLine($"Scraped article -> ID: {id}, Date: {date}, Author: {author}, Title: {title}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }

            //--------------------------------------------------------------------------------
            //
            // New links (after TibiaNews.net redesigned their website around 2010)
            //
            //--------------------------------------------------------------------------------

            // Load all new links and sort them
            links.Clear();
            links.AddRange(File.ReadAllLines("new-links.txt"));
            links.Sort();

            // Remove duplicate links (if any)
            List<string> sortedNew = links.Distinct().ToList();

            // Iterate through each old link
            foreach (string link in sortedNew)
            {
                try
                {
                    // Scrape each link (forum thread)
                    var html = link;
                    HtmlWeb web = new HtmlWeb();
                    var htmlDoc = web.Load(html);

                    // Get the article ID
                    int id = Int32.Parse(Regex.Match(link, @"/(\d+)(?:-[^/]*)?(?:\.html?)?$").Groups[1].Value);

                    // Get the date
                    string dateString = htmlDoc.DocumentNode.SelectSingleNode("//span[@class='createdate']").InnerText.Replace("<br>", "");
                    dateString = Regex.Replace(dateString, @"\r\n?|\n|\t", "");
                    DateTime dateTime = DateTime.ParseExact(dateString, "dddd, dd MMMM yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    string date = dateTime.ToString("yyyy-MM-dd HH:mm:ss");

                    // Get the title
                    string title = htmlDoc.DocumentNode.SelectSingleNode("//a[@class='contentpagetitle']").InnerText.Replace("<br>", "").Replace("&quot;", "");
                    title = Regex.Replace(title, @"\r\n?|\n|\t", "");
                    title = Regex.Replace(title, @"[^\x20-\x7E]", "");

                    // Get the author
                    string author = htmlDoc.DocumentNode.SelectSingleNode("//span[@class='createby']").InnerText.Replace("<br>", "").Replace("&quot;", "");
                    author = Regex.Replace(author, @"\r\n?|\n|\t", "");
                    author = Regex.Replace(author, @"[^\x20-\x7E]", "");

                    string content = "";
                    var articleContentDiv = htmlDoc.DocumentNode.SelectSingleNode(".//div[@class='article-content']");
                    var paragraphs = articleContentDiv.SelectNodes(".//p");
                    foreach (var paragraph in paragraphs) {
                        content += paragraph.OuterHtml;
                    }
                    content = Regex.Replace(content, @"[^\x20-\x7E]", "");

                    // New Article
                    Article article = new Article();
                    article.id = id;
                    article.date = date;
                    article.title = title;
                    article.author = author;
                    article.content = content;

                    // JSON serialize the article
                    string json = JsonSerializer.Serialize(article);

                    // Save article as JSON file
                    long timeNowInSeconds = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
                    string filePath = Path.Combine(Environment.CurrentDirectory, "archive", id + timeNowInSeconds + ".json");
                    if (File.Exists(filePath))
                    {
                        timeNowInSeconds = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
                        File.WriteAllText(string.Concat(Environment.CurrentDirectory, @"\archive\" + id + timeNowInSeconds + ".json"), json);
                    }
                    else
                    {
                        File.WriteAllText(string.Concat(Environment.CurrentDirectory, @"\archive\" + id + timeNowInSeconds + ".json"), json);
                    }

                    Console.WriteLine($"Scraped article -> ID: {id}, Date: {date}, Author: {author}, Title: {title}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        // Article template
        public class Article
        {
            public int id { get; set; }
            public string? date { get; set; }
            public string? title { get; set; }
            public string? author { get; set; }
            public string? content { get; set; }
        }
    }
}
