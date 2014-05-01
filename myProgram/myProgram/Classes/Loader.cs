﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.Linq;
using System.IO;
using OpenPop.Common;
using OpenPop.Mime;
using OpenPop.Pop3;

namespace myProgram
{
    class Loader
    {
        public void GetAttach(DateTime date)//получение аттачментов
        {
            string connectStr = @"Data Source=.\SQLEXPRESS;AttachDbFilename=C:\Users\Маша\Documents\GitHub\kurs_project\myDataBase.mdf;Integrated Security=True;Connect Timeout=30;User Instance=True";
            SqlDataAdapter daBranch = new SqlDataAdapter("Select * from Branch", connectStr);
            SqlDataAdapter daProv = new SqlDataAdapter("Select * from Provisioner", connectStr);
            DataSet ds = new DataSet();
            daBranch.Fill(ds, "Branch");
            daProv.Fill(ds, "Provisioner");
            DataTable dtBranch = ds.Tables["Branch"];
            DataTable dtProv = ds.Tables["Provisioner"];

            var emailBranch = dtBranch.AsEnumerable()
                .Select(t => t.Field<string>("email"));

            var emailProv = dtProv.AsEnumerable()
                .Select(t => t.Field<string>("email"));

            using (Pop3Client client = new Pop3Client())
            {
                client.Connect("pop3.mail.ru", 110, false);
                client.Authenticate("magazin_ulstu_2014@mail.ru", "mag17012014");

                List<string> listIdMsg = client.GetMessageUids();//получаем список id всех писем

                for (int i = 1; i < listIdMsg.Count(); i++)
                {
                    OpenPop.Mime.Message msg = client.GetMessage(i);//получаем письмо по id
                    string address = msg.Headers.From.Address.ToString();//Получаем адрес отправителя
                    DateTime dateMsg = DateTime.Parse(msg.Headers.Date);//Получаем дату сообщения

                    List<MessagePart> listAttach = msg.FindAllAttachments();//Получаем все аттачменты сообщения

                    foreach (var elem in emailBranch)
                    { 
                        if(address.Substring(0, address.Length) == elem && dateMsg.ToString().Substring(0, 10)==date.ToString().Substring(0, 10))
                        {
                            if(listAttach.Count>0)
                            {
                                foreach(MessagePart attach in listAttach)
                                {
                                    string filePath = Path.Combine(@"D:\MailBranch", attach.FileName);
                                    attach.Save(new FileInfo(filePath));
                                }
                            }
                        }
                    }
                    foreach (var elem in emailProv)
                    {
                        if (address.Substring(0, address.Length) == elem && dateMsg.ToString().Substring(0, 10) == date.ToString().Substring(0, 10))
                        {
                            if (listAttach.Count > 0)
                            {
                                foreach (MessagePart attach in listAttach)
                                {
                                    string filePath = Path.Combine(@"D:\MailProvisioner", attach.FileName);
                                    attach.Save(new FileInfo(filePath));
                                }
                            }
                        }
                    }

                }
            }
        }
    }
}
