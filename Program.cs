using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Data.Schema.ScriptDom;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;



namespace SSRSTest
{
    public class ReportInfo
    {
        public string reportInfoPath;
        public string ssrsUrl;

        public ReportInfo(string r, string ssrs)//constructor accepting path to report
        {
            this.reportInfoPath = r;
            this.ssrsUrl = ssrs;
        }

        public ReportInfo()
        {
            this.reportInfoPath = ConfigurationManager.AppSettings["ReportInfoPath"];
            #if(DEV)
                                this.ssrsUrl = ConfigurationManager.AppSettings["DEV_SSRS_WebService"];
            #elif(PRESIT)
                                this.ssrsUrl = ConfigurationManager.AppSettings["PRESIT_SSRS_WebService"];
            #elif(SIT)
                                  this.ssrsUrl = ConfigurationManager.AppSettings["SIT_SSRS_WebService"];
            #elif(PROD)
                                this.ssrsUrl = ConfigurationManager.AppSettings["PROD_SSRS_WebService"];
            #endif
        }

        /// <summary>
        /// See http://www.nunit.org/index.php?p=testCaseSource&r=2.5.9f
        /// For reference
        /// Also https://msdn.microsoft.com/en-us/library/9k7k7cf0.aspx
        /// for info re use of Yield keyword, in summary (from MSDN)
        /// When you use the yield keyword in a statement, you indicate that the method, operator, 
        /// or get accessor in which it appears is an iterator. Using yield to define an iterator removes the need 
        /// for an explicit extra class (the class that holds the state for an enumeration, see IEnumerator<T> for an example) 
        /// when you implement the IEnumerable and IEnumerator pattern for a custom collection type.
        /// </summary>

        public IEnumerable GetReportTestCases
        {
            get
            {
                using (var reports = XmlReader.Create(this.reportInfoPath))
                {
                    while (reports.Read())
                    {
                        if (reports.Name == "report")
                        {
                            int reportNumber;
                            if (!int.TryParse(reports["filename"].Substring(0, 3), out reportNumber))
                            {
                                reportNumber = 0;
                            }

                            string reportPath = string.Concat(reports["subfolder"].ToString().Length > 0 ? "/" : "", reports["subfolder"], "/", reports.ReadElementContentAsString());
                            yield return new TestCaseData(reportNumber, reportPath, this.ssrsUrl);
                        }
                    }
                }
            }

        }
    }
}

namespace SSRSTest
{


    [TestFixture]
    public class ReportRenderTest
    {
        public string ROOT_PATH = ConfigurationManager.AppSettings["RootPath"];

        [Test]
        [TestCaseSource(typeof(ReportInfo), "GetReportTestCases")]
        [Category("SSRSTest")]
        public void DoesReportRender(int reportNumber, string reportPath, string ssrsUrl)
        {
            Console.WriteLine("Testing report: {0}: {1} @ {2}", reportNumber.ToString("000"), reportPath, DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
            bool result = ReportTester.RenderReport(ROOT_PATH + reportPath, ssrsUrl);
            Assert.AreEqual(true, result);
        }
    }

    [TestFixture]
    public class ReportDeploymentTest
    {
        public string ROOT_PATH = ConfigurationManager.AppSettings["RootPath"];
        [Test]
        [TestCaseSource(typeof(ReportInfo), "GetReportTestCases")]
        [Category("SSRSTest")]
        public void DoesReportExist(int reportNumber, string reportPath, string ssrsUrl)
        {
            Console.WriteLine("Testing report: {0}: {1} @ {2}", reportNumber.ToString("000"), reportPath, DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
            bool result = ReportTester.ReportExists(ROOT_PATH + reportPath, ssrsUrl);
            Assert.AreEqual(true, result);
        }

    }

}



