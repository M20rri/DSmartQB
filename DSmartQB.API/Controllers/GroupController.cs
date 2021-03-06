﻿using DSmartQB.CORE.DTOs;
using System;
using System.Linq;
using System.Web.Http;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Web;
using DSmartQB.CORE.Services;
using System.Web.Http.Cors;
using System.Collections.Generic;

namespace DSmartQB.API.Controllers
{
    [Authorize(Roles = "Administrator,Teacher")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class GroupController : ApiController
    {

        [HttpGet, Route("api/ListGroups/{id}")]
        public IHttpActionResult ListGroups([FromUri]int id)
        {
            var result = new GroupService().ListGroups(id);
            return Ok(result);
        }

        [HttpGet, Route("api/LoadGroups")]
        public IHttpActionResult LoadGroups()
        {
            var result = new GroupService().LoadGroups();
            return Ok(result);
        }

        [HttpPost, Route("api/AddGroup")]
        public IHttpActionResult AddGroup([FromBody]GroupAddDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Model");
            }
            var result = new GroupService().AddGroup(model);
            return Ok(result);
        }

        [HttpPost, Route("api/AddBulkGroup")]
        public IHttpActionResult AddBulkGroup()
        {
            var httpRequest = HttpContext.Current.Request;
            var form = httpRequest.Form;
            var ds = new DataSet();
            ReturnMessage result = new ReturnMessage();

            #region Guid

            string path = Path.GetRandomFileName();
            path = path.Replace(".", "");
            string Guid = path.Substring(0, 8);

            #endregion


            string CreatedBy = "";
            try
            {
                if (httpRequest.Files.Count > 0)
                {
                    foreach (string file in httpRequest.Files)
                    {
                        var postedFileBase = httpRequest.Files[file];
                        if (postedFileBase != null)
                        {
                            foreach (string key in form.AllKeys)
                            {
                                CreatedBy = form[key];
                            }

                            string fileExtension = Path.GetExtension(postedFileBase.FileName);
                            if (fileExtension == ".xls" || fileExtension == ".xlsx")
                            {
                                string fileLocation = HttpContext.Current.Server.MapPath("~/Uploads/") + postedFileBase.FileName + Guid;
                                if (File.Exists(fileLocation))
                                {
                                    File.Delete(fileLocation);
                                }
                                postedFileBase.SaveAs(fileLocation);
                                var excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                                if (fileExtension == ".xls")
                                {
                                    excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                                }
                                else if (fileExtension == ".xlsx")
                                {
                                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                                }
                                
                                OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                                excelConnection.Open();
                                var dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                                if (dt == null)
                                {
                                    return null;
                                }
                                var excelSheets = new string[dt.Rows.Count];

                                

                                int t = 0;
                                foreach (DataRow row in dt.Rows)
                                {
                                    excelSheets[t] = row["TABLE_NAME"].ToString();
                                    t++;
                                }

                                var excelConnection1 = new OleDbConnection(excelConnectionString);

                                string query = $"Select * from [{excelSheets[0]}]";

                                using (var dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                                {
                                    dataAdapter.Fill(ds);
                                }
                                int count = Convert.ToInt32(ds.Tables[0].Rows.Count);
                                
                                for (int i = 0; i < count; i++)
                                {
                                    var name = ds.Tables[0].Rows[i][0].ToString();
                                    
                                    GroupAddDto model = new GroupAddDto { CreatedBy = CreatedBy, Name = name };
                                    
                                    result = new GroupService().AddGroup(model);
                                    
                                }
                                return Ok(result);
                            }
                            else
                            {
                                return BadRequest("You must upload excel sheets only");
                            }
                        }
                    }
                }
                return BadRequest("No File Uploaded");
            }
            catch (Exception e)
            {
                return BadRequest(e.InnerException.ToString());
            }


        }

        [HttpPost, Route("api/UpdateGroup")]
        public IHttpActionResult UpdateGroup([FromBody]GroupAddDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Fill Empty Records");
            }
            var result = new GroupService().Update(model);
            return Ok(result);
        }

        [HttpPost, Route("api/DeleteGroup")]
        public IHttpActionResult DeleteGroup([FromBody]Remove remove)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Fill Empty Records");
            }
            var result = new GroupService().Delete(remove.Id);
            return Ok(result);
        }


        [HttpPost, Route("api/DeleteListGroup")]
        public IHttpActionResult DeleteListGroup([FromBody]List<string> remove)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Fill Empty Records");
            }
            var result = new GroupService().DeleteAll(remove);
            return Ok(result);
        }

        [HttpPost, Route("api/AssignUserGroup")]
        public IHttpActionResult AssignUserGroup(GroupUser model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Fill Empty Records");
            }
            var result = new GroupService().AssignGroup(model);
            return Ok(result);
        }



    }
}
