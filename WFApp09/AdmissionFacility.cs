using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFApp09
{


    public class AdmissionFacility
    {
        static private FbConnection fbConnection = null;

        static private ISet<ModelLoginning>  logins = null;

        static private bool _loginAcomplished = false;



        public AdmissionFacility()
        {

        }

        public AdmissionFacility( bool init)
        {
            bool success = false;
            if (init)
            {
                success = initConnectionToDB();
                if (success)
                {
                    _loginAcomplished = true;
                }
            }
        }



        public static bool InitDBConnection(string sServer, string sCharset, string sUserLogin, string sPassword, string sDB, out FbConnection firebirdConnection)
        {
            bool ret = false;
            firebirdConnection = null;

            var strConnection = @"Server=" + sServer + "; User=" + sUserLogin + "; Password=" + sPassword + "; Database=" + sDB + "; Character Set=" + sCharset;

            //var firebirdConnectionStringBuilder = new FbConnectionStringBuilder(@"Server=localhost; User=SYSDBA; Password=masterkey; Database=C:\SSG\PROJECTs\TELET\DB4TELEFONE\sampd_cexs.fdb; Character Set=Win1251");
            var firebirdConnectionStringBuilder = new FbConnectionStringBuilder(strConnection);
            try
            {
                firebirdConnection = new FbConnection(firebirdConnectionStringBuilder.ToString());

                firebirdConnection.Open();

                var firebirdInfo = new FbDatabaseInfo(firebirdConnection); //информация о БД

                Console.WriteLine("Database Info is {0}", firebirdInfo.ToString());

                ret = true;
                ////////////////////////////////////////////////////////
                // devel2.frunze.local:sampd_cexs
                //fb.ConnectionString = sServer + ":" + sDB;
                //fb.Charset = sCharset;
                //fb.UserID = sUserLogin;
                //fb.Password = sPassword;

                //var fbconn = new FbConnection(fb.ToString());

                //Type typ = fbconn.GetType();

                //Console.WriteLine(typ.ToString());
                ////////////////////////////////////////////////////////
                return ret;
            }
            catch (FbException Ex)
            {
                Console.WriteLine("Error occured during connection establishing ...");
                throw;
            }
        }




        public static bool getDataReaderForSqlSelect(string sSqlSelect, FbConnection connection, out FbDataReader firebirdDataReader)
        {
            bool ret = false;
            FbTransaction transaction;
            FbCommand selectSQLCommand;
            firebirdDataReader = null;
            if (connection.State == ConnectionState.Closed) //если соединение закрыто - откроем его; Перечисление ConnectionState содержит состояния соединения (подключено/отключено)
                connection.Open();
            if (connection == null)
            {
                return ret;
            }
            try
            {
                selectSQLCommand = new FbCommand(sSqlSelect, connection);

                transaction = connection.BeginTransaction(); //стартуем транзакцию; стартовать транзакцию можно только для открытой базы (т.е. мутод Open() уже был вызван ранее, иначе ошибка)
                selectSQLCommand.Transaction = transaction;

                firebirdDataReader = selectSQLCommand.ExecuteReader(); //для запросов, которые возвращают результат в виде набора данных надо использоваться метод ExecuteReader()

                ret = (firebirdDataReader != null && firebirdDataReader.IsClosed == false) ? true : false;

                return ret;
            }
            catch (FbException Ex)
            {
                Console.WriteLine("Error occured while getting Firebird DataReader ...");
                throw;
            }
        }



        private static bool readAllUsersLogginsPassword(in FbDataReader firebirdDataReader, out ISet<ModelLoginning> modelLoginnings)
        {
            modelLoginnings = new  HashSet<ModelLoginning>();
            bool ret = false;
            if (firebirdDataReader == null)
            {
                return ret;
            }

            if (firebirdDataReader.HasRows)
            {
                int i = 0;
                while (firebirdDataReader.Read())
                {

                    int iFieldsCount = firebirdDataReader.FieldCount;
                    if (i > 0)
                    {
                        ModelLoginning modelRecord = new ModelLoginning();
                        for (int ii = 0; ii < iFieldsCount; ii++)
                        {
                            string nameOfCol = firebirdDataReader.GetName(ii);
                            if (nameOfCol.StartsWith("NAME_USER"))
                            {
                                modelRecord.UserLogin = (string)firebirdDataReader.GetValue(ii);
                            } else if (nameOfCol.StartsWith("PASS"))
                            {
                                modelRecord.UserPassword = (string)firebirdDataReader.GetValue(ii);
                            }
                        }
                        modelLoginnings.Add(modelRecord);
                    }
                    i++;
                }
                ret = true;
            }
            return ret;
        }



        private static bool initConnectionToDB()
         {
            bool result = false;

            string strSelectQuery = @"SELECT * FROM SYS_USER;"; //задаем запрос на выборку логины пользователей
            FbDataReader firebirdDataReader;

            try
            {
                // (string sServer, string sCharset, string sUserLogin, string sPassword, string sDB)
                //result = InitDBConnection("devel2.frunze.local:sampd_cexs", "WIN1251", "STAVITSKIY_S", "q1w2e3", "sampd_cexs", out fbConnection);

                //FbConnectionStringBuilder(@"Server=localhost; User=SYSDBA; Password=masterkey; Database=C:\SSG\PROJECTs\TELET\DB4TELEFONE\sampd_cexs.fdb; Character Set=Win1251");

                result = InitDBConnection("localhost", "WIN1251", "SYSDBA", "masterkey", "C:\\SSG\\PROJECTs\\TELET\\DB4TELEFONE\\sampd_cexs.fdb", out fbConnection);
                result &= getDataReaderForSqlSelect(strSelectQuery, fbConnection, out firebirdDataReader);
                result &= readAllUsersLogginsPassword(firebirdDataReader, out logins);
            }
            catch (FbException ex)
            {
                Type tOfEx = ex.GetType();
                Console.WriteLine("Error with Firebird Db operation ...");
                Console.WriteLine("Error Type --- {0}", tOfEx);
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Type tOfEx = ex.GetType();
                Console.WriteLine("Error Type is {0}", tOfEx);
                Console.WriteLine("============================");
                Console.WriteLine("Error message is {0}", ex.Message);
            }
            finally
            {
                if (fbConnection != null && fbConnection.State != ConnectionState.Closed)
                {
                    fbConnection.Close();
                    Console.WriteLine("Подключение закрыто...");
                }
            }
            return result;
        }
        public bool tryAccess( string sUser,  string sPass, out ulong ulToken)
        {
            ulToken = 0UL;
            bool resultOfConnection = false;

            if (_loginAcomplished) {
                resultOfConnection = initConnectionToDB();
                _loginAcomplished = !_loginAcomplished;
            }

            if (resultOfConnection)
            {
                var resTokens = from el in logins where el.UserLogin == sUser && el.UserPassword == sPass select el.LoginToken;
                Console.WriteLine("Type of this {0}", resTokens.GetType());
                foreach (var et in resTokens)
                {
                    if ( et > 0) {
                        ulToken = et;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool tryAccess(string sUser, ulong ulToken)
        {
            if(_loginAcomplished && logins != null && logins.Count > 0)
            {
                var resTokens = from el in logins where el.UserLogin == sUser && el.LoginToken == ulToken select el.LoginToken;
                Console.WriteLine(" type is {0}", resTokens.GetType());
                foreach(var et in resTokens)
                {
                    if (et == ulToken)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public bool tryAccess(ModelLoginning modelLogin)
        {
            if (modelLogin == null) return false;

            return this.tryAccess(modelLogin.UserLogin, modelLogin.LoginToken);
        }
    }
}
