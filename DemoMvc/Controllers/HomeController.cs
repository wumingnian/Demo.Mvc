using CADImport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using WebCAD;

namespace DemoMvc.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            ViewBag.DrawingFile = Path.Combine(Server.MapPath("~/App_Data"), "aa.dwg");
            ViewBag.DrawingID = DrawingManager.Add(null, ViewBag.DrawingFile).Id;
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {

            if (file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var dir = Directory.CreateDirectory(Server.MapPath("~/App_Data/uploads")).FullName;
                var path = Path.Combine(dir, fileName);
                file.SaveAs(path);

                ViewBag.DrawingFile = path;
                ViewBag.DrawingID = DrawingManager.Add(null, path).Id;
            }

            return View();
        }

        public ActionResult LoadFromHDD()
        {
            ViewBag.DrawingFile = Path.Combine(Server.MapPath("~/App_Data"), "ford_cobra.dwg");
            ViewBag.DrawingID = DrawingManager.Add(null, ViewBag.DrawingFile).Id;
            return View("Index");
        }

        public ActionResult LoadFromWeb(String fileUrl)
        {
            string file = Path.GetTempPath() + Guid.NewGuid().ToString() + "_floorplan.dwg";
            System.Diagnostics.Debug.WriteLine("fileUrl"+ fileUrl);
            // string url = "http://61.189.238.126:8888/safe-webs/common/file/download?id=75d1fff6082ce19be050a8c014020436";
            string url = fileUrl;
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, file);

                ViewBag.DrawingFile = file;
                ViewBag.DrawingID = DrawingManager.Add(null, ViewBag.DrawingFile).Id;
            }
            return View("Index");
        }

        public struct AttributesExport
        {
            public string BlockName;
            public Dictionary<string, string> Tags;
        }

        public ActionResult GetCSV(string guid)
        {
            string resp = "";

            DrawingState ds = DrawingManager.Get(guid);
            if (ds != null)
            {
                List<AttributesExport> toExcel = new List<AttributesExport>();

                if (DrawingManager.Engine == DrawingEngine.CADNET)
                {
                    CADImage cadImage = ds.Drawing.GetInstance() as CADImage;
                    foreach (CADEntity ent in cadImage.Converter.Entities)
                        if ((ent is CADInsert) && ((ent as CADInsert).Attribs.Count == 3))
                        {
                            AttributesExport atrExp = new AttributesExport();
                            atrExp.Tags = new Dictionary<string, string>();
                            foreach (CADAttrib attr in (ent as CADInsert).Attribs)
                                atrExp.Tags.Add(attr.Tag, attr.Value);
                            atrExp.BlockName = (ent as CADInsert).Block.Name;
                            toExcel.Add(atrExp);
                        }
                }
                else
                {
                    string xml = ds.Drawing.ProcessXML("<?xml version=\"1.0\" encoding=\"UTF-8\"?><cadsofttools version=\"2\"><get mode=\"5\" /></cadsofttools>");

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                    XmlNodeList nodes = doc.SelectNodes("//cstInsert");

                    foreach (XmlNode node in nodes)
                        if (node.ChildNodes.Count == 3)
                        {
                            AttributesExport attrExp = new AttributesExport();
                            attrExp.Tags = new Dictionary<string, string>();
                            XmlAttribute attr = node.Attributes["BlockName"];
                            if (attr != null)
                                attrExp.BlockName = attr.Value;
                            foreach (XmlNode cNode in node.ChildNodes)
                            {
                                XmlAttribute atrTag = cNode.Attributes["Tag"];
                                XmlAttribute atrValue = cNode.Attributes["Value"];
                                if ((atrTag != null) && (atrValue != null))
                                    attrExp.Tags.Add(atrTag.Value, atrValue.Value);
                            }
                            toExcel.Add(attrExp);
                        }
                }

                foreach (AttributesExport attr in toExcel)
                {
                    resp += attr.BlockName + "; ";
                    foreach (var tag in attr.Tags)
                        resp += tag.Key + "; " + tag.Value + "; ";
                    resp += "\r\n";
                }
            }
            Response.AddHeader("Content-Disposition", "attachment;filename=attribs.csv");
            return Content(resp, "text/csv");
        }
    }
}
