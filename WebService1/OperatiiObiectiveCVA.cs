using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using System.Globalization;

namespace WebService1
{
    public class OperatiiObiectiveCVA
    {

        public string adaugaObiectiv(string dateObiective, string beneficiari, string stadii)
        {


            string statusCode = "1";



            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Objective> listObiective = serializer.Deserialize<List<Objective>>(dateObiective);
            List<Beneficiar> listBeneficiari = serializer.Deserialize<List<Beneficiar>>(beneficiari);
            List<ObjectivePhase> listStadii = serializer.Deserialize<List<ObjectivePhase>>(stadii);

            OracleConnection connection = new OracleConnection();
            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

            OracleParameter idCmd = null;

            try
            {

                OracleCommand cmd = connection.CreateCommand();

                cmd.Transaction = transaction;
                string query = "";

                query = " delete from sapprd.ztbl_objectives where cva_code =:cva_code";
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":cva_code", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = listObiective[0].cvaCode;

                cmd.ExecuteNonQuery();

                query = " delete from sapprd.ztbl_beneficiari where cva_code =:cva_code";
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":cva_code", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = listObiective[0].cvaCode;

                cmd.ExecuteNonQuery();


                query = " delete from sapprd.ztbl_obj_ph_cva where cva_code =:cva_code";
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":cva_code", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = listObiective[0].cvaCode;

                cmd.ExecuteNonQuery();





                foreach (Objective obiectiv in listObiective)
                {


                    query = " insert into sapprd.ztbl_objectives(mandt, ora_id, id, type_id, cva_code, region_id, name, creation_date, beneficiary_id, beneficiaty_type, " +
                          " authorization_start, authorization_end, estimation_value, address, zip, gps, stage_id, phase_id, expiration_phase, status, status_id, category_id, " +
                          " nume_executant, cui_executant, nrc_executant, tel_benef, filiala, numeMeserias, prenMeserias, telMeserias) values " +
                          " ('900', pk_clp.nextval, :id, :type_id, :cva_code, :region_id, :name, :creation_date, :beneficiary_id, :beneficiary_type,  " +
                          " :authorization_start, :authorization_end, :estimation_value, :address, :zip, :gps, :stage_id, :phase_id, :expiration_phase, :status, :status_id, " +
                          " :category_id, :nume_executant, :cui_executant, :nrc_executant, :tel_benef, :filiala, :numeMeserias, :prenMeserias, :telMeserias) " +
                          " returning ora_id into :oraid ";


                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":id", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = obiectiv.id;

                    cmd.Parameters.Add(":type_id", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = obiectiv.typeId;

                    cmd.Parameters.Add(":cva_code", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = obiectiv.cvaCode;

                    cmd.Parameters.Add(":region_id", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = obiectiv.regionID;

                    cmd.Parameters.Add(":name", OracleType.NVarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = obiectiv.name;

                    cmd.Parameters.Add(":creation_date", OracleType.NVarChar, 45).Direction = ParameterDirection.Input;
                    cmd.Parameters[5].Value = obiectiv.creationDate;

                    cmd.Parameters.Add(":beneficiary_id", OracleType.NVarChar, 45).Direction = ParameterDirection.Input;
                    cmd.Parameters[6].Value = obiectiv.beneficiaryId;

                    cmd.Parameters.Add(":beneficiary_type", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[7].Value = obiectiv.beneficiaryType;

                    cmd.Parameters.Add(":authorization_start", OracleType.NVarChar, 45).Direction = ParameterDirection.Input;
                    cmd.Parameters[8].Value = obiectiv.authorizationStart;

                    cmd.Parameters.Add(":authorization_end", OracleType.NVarChar, 45).Direction = ParameterDirection.Input;
                    cmd.Parameters[9].Value = obiectiv.authorizationEnd;

                    cmd.Parameters.Add(":estimation_value", OracleType.Number, 13).Direction = ParameterDirection.Input;
                    if (obiectiv.estimationValue.Contains(","))
                        cmd.Parameters[10].Value = decimal.Parse(obiectiv.estimationValue, new NumberFormatInfo() { NumberDecimalSeparator = "," });
                    else
                        cmd.Parameters[10].Value = obiectiv.estimationValue;

                    cmd.Parameters.Add(":address", OracleType.NVarChar, 90).Direction = ParameterDirection.Input;
                    cmd.Parameters[11].Value = obiectiv.address;

                    cmd.Parameters.Add(":zip", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[12].Value = obiectiv.zip;

                    cmd.Parameters.Add(":gps", OracleType.NVarChar, 90).Direction = ParameterDirection.Input;
                    cmd.Parameters[13].Value = obiectiv.gps;

                    cmd.Parameters.Add(":stage_id", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[14].Value = obiectiv.stageId;

                    cmd.Parameters.Add(":phase_id", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[15].Value = obiectiv.phaseId;

                    cmd.Parameters.Add(":expiration_phase", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[16].Value = obiectiv.expirationPhase;

                    cmd.Parameters.Add(":status", OracleType.NVarChar, 45).Direction = ParameterDirection.Input;
                    cmd.Parameters[17].Value = obiectiv.status;

                    cmd.Parameters.Add(":status_id", OracleType.NVarChar, 9).Direction = ParameterDirection.Input;
                    cmd.Parameters[18].Value = obiectiv.statusId;

                    cmd.Parameters.Add(":category_id", OracleType.NVarChar, 9).Direction = ParameterDirection.Input;
                    cmd.Parameters[19].Value = obiectiv.categoryId;

                    cmd.Parameters.Add(":nume_executant", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[20].Value = obiectiv.numeExecutant;

                    cmd.Parameters.Add(":cui_executant", OracleType.NVarChar, 15).Direction = ParameterDirection.Input;
                    cmd.Parameters[21].Value = obiectiv.cuiExecutant;

                    cmd.Parameters.Add(":nrc_executant", OracleType.NVarChar, 15).Direction = ParameterDirection.Input;
                    cmd.Parameters[22].Value = obiectiv.nrcExecutant;

                    cmd.Parameters.Add(":tel_benef", OracleType.NVarChar, 15).Direction = ParameterDirection.Input;
                    cmd.Parameters[23].Value = obiectiv.telBenef;

                    cmd.Parameters.Add(":filiala", OracleType.NVarChar, 9).Direction = ParameterDirection.Input;
                    cmd.Parameters[24].Value = obiectiv.filiala;

                    cmd.Parameters.Add(":numeMeserias", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[25].Value = obiectiv.numeMeserias;

                    cmd.Parameters.Add(":prenMeserias", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[26].Value = obiectiv.prenMeserias;

                    cmd.Parameters.Add(":telMeserias", OracleType.NVarChar, 36).Direction = ParameterDirection.Input;
                    cmd.Parameters[27].Value = obiectiv.telMeserias;

                    idCmd = new OracleParameter("oraid", OracleType.Number);
                    idCmd.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(idCmd);


                    cmd.ExecuteNonQuery();


                }






                foreach (Beneficiar beneficiar in listBeneficiari)
                {
                    query = " insert into sapprd.ztbl_beneficiari (mandt, ora_id, cui, id, region_id, name, type, nr_rc, cnp, status, cva_code) values " +
                            " ('900', :ora_id, :cui, :id, :region_id, :name, :type, :nr_rc, :cnp, :status, :cva_code )";


                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":ora_id", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = idCmd.Value;

                    cmd.Parameters.Add(":cui", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = beneficiar.cui;

                    cmd.Parameters.Add(":id", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = beneficiar.id;

                    cmd.Parameters.Add(":region_id", OracleType.NVarChar, 15).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = beneficiar.region_id;

                    cmd.Parameters.Add(":name", OracleType.NVarChar, 60).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = beneficiar.name;

                    cmd.Parameters.Add(":type", OracleType.NVarChar, 15).Direction = ParameterDirection.Input;
                    cmd.Parameters[5].Value = beneficiar.type;

                    cmd.Parameters.Add(":nr_rc", OracleType.NVarChar, 60).Direction = ParameterDirection.Input;
                    cmd.Parameters[6].Value = beneficiar.nr_rc;

                    cmd.Parameters.Add(":cnp", OracleType.NVarChar, 45).Direction = ParameterDirection.Input;
                    cmd.Parameters[7].Value = beneficiar.cnp;

                    cmd.Parameters.Add(":status", OracleType.NVarChar, 15).Direction = ParameterDirection.Input;
                    cmd.Parameters[8].Value = beneficiar.status;

                    cmd.Parameters.Add(":cva_code", OracleType.NVarChar, 15).Direction = ParameterDirection.Input;
                    cmd.Parameters[9].Value = listObiective[0].cvaCode;



                    cmd.ExecuteNonQuery();

                }



                foreach (ObjectivePhase stadiu in listStadii)
                {
                    query = " insert into sapprd.ztbl_obj_ph_cva (mandt, ora_id, id, phase_id, objective_id, days_nr, phase_start, phase_end, cva_code) values " +
                            " ('900', :ora_id, :id, :phase_id, :objective_id, :days_nr, :phase_start, :phase_end, :cva_code ) ";


                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":ora_id", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = idCmd.Value;

                    cmd.Parameters.Add(":id", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = stadiu.id;

                    cmd.Parameters.Add(":phase_id", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = stadiu.phase_id;

                    cmd.Parameters.Add(":objective_id", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = stadiu.objective_id;

                    cmd.Parameters.Add(":days_nr", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = stadiu.days_nr;

                    cmd.Parameters.Add(":phase_start", OracleType.NVarChar, 45).Direction = ParameterDirection.Input;
                    cmd.Parameters[5].Value = stadiu.phase_start;

                    cmd.Parameters.Add(":phase_end", OracleType.NVarChar, 45).Direction = ParameterDirection.Input;
                    cmd.Parameters[6].Value = stadiu.phase_end;

                    cmd.Parameters.Add(":cva_code", OracleType.NVarChar, 15).Direction = ParameterDirection.Input;
                    cmd.Parameters[7].Value = listObiective[0].cvaCode;

                    cmd.ExecuteNonQuery();

                }


                transaction.Commit();

                if (cmd != null)
                    cmd.Dispose();

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ErrorHandling.sendErrorToMail(ex.ToString() );
                statusCode = "0";
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }



            return statusCode;
        }




        public string getObiective(string tipUser, string codUser, string filiala)
        {




            string strObiective = "", strBeneficiari = "", strPhases = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            OracleTransaction transaction = null;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                cmd = connection.CreateCommand();

                cmd.Transaction = transaction;


                string condAgent = "";

                if (tipUser.Equals("CV", StringComparison.OrdinalIgnoreCase))
                    condAgent = " and b.cva_code =:cva_code ";


                string condFiliala = "";
                if (filiala != null && filiala.Trim().Length > 0)
                    condFiliala = " and b.filiala = '" + filiala + "' ";


                string sqlString = " select b.id,b.type_id, b.cva_code, b.region_id, b.name, b.creation_date, b.beneficiary_id, b.beneficiaty_type, b.authorization_start, b.authorization_end, " +
                                  " b.estimation_value, b.address, b.zip, b.gps, b.stage_id, b.phase_id, b.expiration_phase, b.status, b.status_id, b.category_id, b.nume_executant, b.cui_executant, " +
                                  " b.nrc_executant, b.tel_benef, b.filiala, b.ora_id, b.numeMeserias, b.prenMeserias, b.telMeserias " +
                                  " from sapprd.ztbl_objectives  b where 1 = 1 " + condAgent + condFiliala + " order by id ";

                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();


                

                if (tipUser.Equals("CV", StringComparison.OrdinalIgnoreCase))
                {
                    cmd.Parameters.Add(":cva_code", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codUser;
                }

                oReader = cmd.ExecuteReader();


                List<Objective> listObiective = new List<Objective>();
                Objective obiectiv = null;


                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        obiectiv = new Objective();
                        obiectiv.id = oReader.GetInt32(0).ToString();
                        obiectiv.typeId = oReader.GetInt32(1).ToString();
                        obiectiv.cvaCode = oReader.GetString(2);
                        obiectiv.regionID = oReader.GetString(3);
                        obiectiv.name = oReader.GetString(4);
                        obiectiv.creationDate = oReader.GetString(5);
                        obiectiv.beneficiaryId = oReader.GetString(6);
                        obiectiv.beneficiaryType = oReader.GetString(7);
                        obiectiv.authorizationStart = oReader.GetString(8);
                        obiectiv.authorizationEnd = oReader.GetString(9);
                        obiectiv.estimationValue = oReader.GetDouble(10).ToString();
                        obiectiv.address = oReader.GetString(11);
                        obiectiv.zip = oReader.GetInt32(12).ToString();
                        obiectiv.gps = oReader.GetString(13);
                        obiectiv.stageId = oReader.GetInt32(14).ToString();
                        obiectiv.phaseId = oReader.GetInt32(15).ToString();
                        obiectiv.expirationPhase = oReader.GetString(16);
                        obiectiv.status = oReader.GetString(17);
                        obiectiv.statusId = oReader.GetString(18);
                        obiectiv.categoryId = oReader.GetString(19);
                        obiectiv.numeExecutant = oReader.GetString(20);
                        obiectiv.cuiExecutant = oReader.GetString(21);
                        obiectiv.nrcExecutant = oReader.GetString(22);
                        obiectiv.telBenef = oReader.GetString(23);
                        obiectiv.filiala = oReader.GetString(24);
                        obiectiv.oraid = oReader.GetInt32(25).ToString();
                        obiectiv.numeMeserias = oReader.GetString(26).ToString();
                        obiectiv.prenMeserias = oReader.GetString(27).ToString();
                        obiectiv.telMeserias = oReader.GetString(28).ToString();
                        listObiective.Add(obiectiv);

                    }


                }


                strObiective = serializer.Serialize(listObiective);


              

                Beneficiar unBeneficiar = null;
                List<Beneficiar> listBeneficiari = new List<Beneficiar>();

                foreach (Objective unObiectiv in listObiective)
                {
                    sqlString = " select a.id, a.region_id, a.name, a.type, a.cui, a.nr_rc, a.cnp, a.status, a.cva_code from sapprd.ztbl_beneficiari a " +
                                " where a.cva_code =:codAgent and id=:benefId ";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sqlString;
                    cmd.Parameters.Clear();


                    cmd.Parameters.Add(":codAgent", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = unObiectiv.cvaCode;

                    cmd.Parameters.Add(":benefId", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = Int32.Parse(unObiectiv.beneficiaryId);



                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            unBeneficiar = new Beneficiar();
                            unBeneficiar.id = oReader.GetInt32(0).ToString();
                            unBeneficiar.region_id = oReader.GetString(1);
                            unBeneficiar.name = oReader.GetString(2);
                            unBeneficiar.type = oReader.GetString(3);
                            unBeneficiar.cui = oReader.GetString(4);
                            unBeneficiar.nr_rc = oReader.GetString(5);
                            unBeneficiar.cnp = oReader.GetString(6);
                            unBeneficiar.status = oReader.GetString(7);
                            unBeneficiar.cvaCode = oReader.GetString(8);

                            bool alreadyExists = listBeneficiari.Any(x => x.id == unBeneficiar.id);

                            if (!alreadyExists)
                                listBeneficiari.Add(unBeneficiar);
                        }
                    }


                }

                strBeneficiari = serializer.Serialize(listBeneficiari);




                ObjectivePhase phase = null;
                List<ObjectivePhase> listPhases = new List<ObjectivePhase>();

                foreach (Objective unObiectiv in listObiective)
                {
                    sqlString = " select a.id, a.phase_id, a.objective_id, a.days_nr, a.phase_start, a.phase_end, a.cva_code from sapprd.ztbl_obj_ph_cva a " +
                               " where a.cva_code =:codAgent and objective_id=:phaseId";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sqlString;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":codAgent", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = unObiectiv.cvaCode;

                    cmd.Parameters.Add(":phaseId", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = Int32.Parse(unObiectiv.id);



                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            phase = new ObjectivePhase();
                            phase.id = oReader.GetInt32(0);
                            phase.phase_id = oReader.GetInt32(1);
                            phase.objective_id = oReader.GetInt32(2);
                            phase.days_nr = oReader.GetInt32(3);
                            phase.phase_start = oReader.GetString(4);
                            phase.phase_end = oReader.GetString(5);
                            phase.cvaCode = oReader.GetString(6);

                            listPhases.Add(phase);
                        }

                    }


                }

                strPhases = serializer.Serialize(listPhases);

                transaction.Commit();

               

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);

            }


            TabeleObiectiveCVA tabele = new TabeleObiectiveCVA();
            tabele.obiective = strObiective;
            tabele.beneficiari = strBeneficiari;
            tabele.stadii = strPhases;

            string strTabele = serializer.Serialize(tabele);

            

            return strTabele;
        }


        public bool schimbaAgentObiecticeCVA(string codAgentVechi, string codAgentNou)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;

            bool success = true;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string query = " update sapprd.ztbl_objectives set cva_code=:agentNou where cva_code=:agentVechi ";

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":agentNou", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgentNou;

                cmd.Parameters.Add(":agentVechi", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAgentVechi;

                cmd.ExecuteNonQuery();



                query = " update sapprd.ztbl_beneficiari set cva_code=:agentNou where cva_code=:agentVechi ";

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":agentNou", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgentNou;

                cmd.Parameters.Add(":agentVechi", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAgentVechi;

                cmd.ExecuteNonQuery();

                query = " update sapprd.ztbl_obj_ph_cva set cva_code=:agentNou where cva_code=:agentVechi ";

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":agentNou", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgentNou;

                cmd.Parameters.Add(":agentVechi", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAgentVechi;

                cmd.ExecuteNonQuery();




            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                success = false;
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }

            return success;
        }



    }
}