using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using WebService1.WebServiceSalarizareAV;

using System.Data.OracleClient;
using System.Data;

namespace WebService1
{
    public class Salarizare
    {
        public String getSalarizareAV(string codAgent, string departament, string filiala, string dataRap)
        {

            string serResult = "";
            string localDep = departament.StartsWith("04") ? Service1.getDepartAgent(codAgent) : departament;

            WebServiceSalarizareAV.ZWBS_SAL_AV webService = new ZWBS_SAL_AV();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(DatabaseConnections.getUser(), DatabaseConnections.getPass());
            webService.Credentials = nc;
            webService.Timeout = 300000;

            WebServiceSalarizareAV.ZgetSalav inParam = new WebServiceSalarizareAV.ZgetSalav();


            WebServiceSalarizareAV.Ztmarja[] zMarja = new Ztmarja[1];
            WebServiceSalarizareAV.ZtprsSapreport[] zTPR = new ZtprsSapreport[1];
            WebServiceSalarizareAV.Zstcf[] zTCF = new Zstcf[1];
            WebServiceSalarizareAV.Zspenalty[] zPenalty = new Zspenalty[1];
            WebServiceSalarizareAV.Zsalav3[] zFinal = new Zsalav3[1];
            WebServiceSalarizareAV.ZprocincSapreport[] zProcente = new ZprocincSapreport[1];


            WebServiceSalarizareAV.Zpernr[] codAgenti = new Zpernr[3];
            codAgenti[0] = new Zpernr();
            codAgenti[0].Pernr = codAgent;


            inParam.An = Utils.getYearFromStrDate(dataRap);
            inParam.Divizie = localDep;
            inParam.Luna = Utils.getMonthFromStrDate(dataRap);
            inParam.Ul = filiala;
            inParam.ItPernr = codAgenti;

            inParam.GtMarjaAv = zMarja;
            inParam.GtTprsdsAv = zTPR;
            inParam.GtTcfAv = zTCF;
            inParam.GtPenalty99 = zPenalty;
            inParam.GtOuttabAv = zFinal;
            inParam.GtProcenteAv = zProcente;

            WebServiceSalarizareAV.ZgetSalavResponse response = new ZgetSalavResponse();

            response = webService.ZgetSalav(inParam);

            int lenTpr = response.GtTprsdsAv.Length;

            List<VenitTPR> listTpr = new List<VenitTPR>();

            for (int i = 0; i < lenTpr; i++)
            {
                VenitTPR venitTpr = new VenitTPR();

                venitTpr.codN2 = response.GtTprsdsAv[i].Codnivel2;
                venitTpr.numeN2 = response.GtTprsdsAv[i].Descriere;
                venitTpr.venitGrInc = response.GtTprsdsAv[i].Venitgi.ToString();
                venitTpr.pondere = response.GtTprsdsAv[i].Pondere.ToString();
                venitTpr.targetPropCant = response.GtTprsdsAv[i].Targetcant.ToString();
                venitTpr.targetRealCant = response.GtTprsdsAv[i].Targetcantr.ToString();
                venitTpr.um = response.GtTprsdsAv[i].Vrkme;
                venitTpr.targetPropVal = response.GtTprsdsAv[i].Targetval.ToString();
                venitTpr.targetRealVal = response.GtTprsdsAv[i].Targetvalr.ToString();
                venitTpr.realizareTarget = response.GtTprsdsAv[i].Realizaretarget.ToString();
                venitTpr.targetPonderat = response.GtTprsdsAv[i].Targetponderat.ToString();
                listTpr.Add(venitTpr);

            }

            int lenTcf = response.GtTcfAv.Length;

            List<VenitTCF> listTcf = new List<VenitTCF>();

            for (int i = 0; i < lenTcf; i++)
            {
                VenitTCF venitTcf = new VenitTCF();
                venitTcf.venitGrInc = response.GtTcfAv[i].VenitTpr.ToString();
                venitTcf.targetPropus = response.GtTcfAv[i].TargetP.ToString();
                venitTcf.targetRealizat = response.GtTcfAv[i].TargetR.ToString();
                venitTcf.coefAfectare = response.GtTcfAv[i].CoefAf.ToString();
                venitTcf.venitTcf = response.GtTcfAv[i].VenitTcf.ToString();
                listTcf.Add(venitTcf);

            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            string serTpr = serializer.Serialize(listTpr);
            string serTcf = serializer.Serialize(listTcf);

            SalarizareAv salarizareAv = new SalarizareAv();
            salarizareAv.venitTpr = serTpr;
            salarizareAv.venitTcf = serTcf;

            serResult = serializer.Serialize(salarizareAv);

            webService.Dispose();

            return serResult;

        }


        public static BazaSalariala getBazaSalariala(OracleConnection connection, string nrCmd, string tipCalcul)
        {

            BazaSalariala bazaSalariala = new BazaSalariala();

            OracleDataReader oReader = null;
            OracleCommand cmd = null;

            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string lastYear = (cDate.Year - 1).ToString();

            double valTotal = 0;

            try
            {
                
                cmd = connection.CreateCommand();


                cmd.CommandText = " select  v.matnr, v.netwr val_art, " +
                                   " nvl(case  " +
                                   " when v.WAVWR = 0 and h.auart = 'ZTP' then(select sum(rfwrt) from sapprd.vbfa f where f.mandt = '900' and f.vbelv = v.vbeln and f.posnv = v.posnr and f.vbtyp_n = 'V') " +
                                   " else WAVWR end, 0) cost_marfa, " +
                                  " nvl((select coef_corr from sapprd.zmarja_coef k where k.mandt = '900' and k.zan = :anCurent and " +
                                  " matnr = d.cod ),0) coef_corr_art, " +
                                  " nvl((select coef_corr from sapprd.zmarja_coef k where k.mandt = '900' and k.zan = :anCurent and " +
                                  " matkl = (select  sintetic from articole where cod = d.cod) and nvl(length(trim(matnr)), 0)= 0),0) coef_corr_sint, " +
                                  " nvl((select case " +
                                  " when a.t1a_proc + a.t1d_proc > nvl(p.proc_comp, 0) and nvl(p.proc_comp, 0) <> 0 and h.auart = 'ZTP' then a.t1a_proc* p.proc_mics / 100 || '#' || a.t1d_proc * p.proc_mics / 100 " +
                                  " else a.t1a_proc || '#' || a.t1d_proc end procente " +
                                  " from sapprd.ZPROC_T1 a, websap.articole b, sapprd.zdesc_proc p " +
                                  " where a.prctr = v.prctr and a.anv = :an and a.luna = '00' and a.matkl = b.sintetic and a.spart = substr(b.grup_vz, 0, 2) " +
                                  " and b.cod = d.cod and '900' = p.mandt(+) and b.sintetic = p.matkl(+)),'0#0') T12, " +
                                  " h.cod_agent, " +
                                  " nvl(t.matnr,'-1') art_taxa, nvl(t.ul,'-1') ul_taxa, nvl(t.tip,'-1') tip_taxa, " +
                                  " h.fact_red, d.procent_redb " +
                                  " from sapprd.vbap v, sapprd.zcomhead_tableta h, sapprd.zcomdet_tableta d, sapprd.zarticol_trap  t " +
                                  " where v.mandt = '900' and h.mandt = '900' and d.mandt = '900' and h.id =:idCmd and v.vbeln = h.nrcmdsap " +
                                  " and v.matkl not in (select matkl from sapprd.zexc_salarizare where mandt='900' ) " +
                                  " and h.id = d.id and d.cod = v.matnr and t.mandt(+)='900' and t.matnr(+) = d.cod ";




                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":anCurent", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = year;

                cmd.Parameters.Add(":an", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = lastYear;

                cmd.Parameters.Add(":idCmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = Int32.Parse(nrCmd);

                oReader = cmd.ExecuteReader();

                double valPoz = 0;
                double procT1 = 0;
                double procT2 = 0;
                double disc_furn = 0;
                double procDiscFurn = 0;
                string codAgent = "";
                double valArt = 0;
                double valFTVA = 0;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        valFTVA += oReader.GetDouble(1);
                        codAgent = oReader.GetString(6);

                        if (!oReader.GetString(7).Equals("-1"))
                        {
                            if (oReader.GetString(8).ToUpper().Equals("BV90") || oReader.GetString(9).ToUpper().Equals("TAXA"))
                                valPoz = 0;
                            else
                            {
                                double valTranspCmd = oReader.GetDouble(1);
                                double valMinTransp = 0;

                                cmd.CommandText = " select nvl(min(x.val_ftva),0) val_min from sapprd.ZTAXA_TRAP x where x.mandt = '900' " +
                                                  " and x.datab <= to_char(sysdate, 'yyyymmdd') and x.datbi >= to_char(sysdate, 'yyyymmdd') and val_ftva <=:valTranspCmd ";

                                cmd.CommandType = System.Data.CommandType.Text;
                                cmd.Parameters.Clear();

                                cmd.Parameters.Add(":valTranspCmd", OracleType.Double, 30).Direction = ParameterDirection.Input;
                                cmd.Parameters[0].Value = valTranspCmd;

                                OracleDataReader oReader1 = cmd.ExecuteReader();

                                if (oReader1.HasRows)
                                {
                                    oReader1.Read();
                                    valMinTransp = oReader1.GetDouble(0);
                                }

                                valPoz = valTranspCmd - valMinTransp;

                                oReader1.Close();
                                oReader1.Dispose();

                            }

                        }
                        else
                        {

                            string[] procT12 = oReader.GetString(5).Split('#');
                            procT1 = Double.Parse(procT12[0]);
                            procT2 = Double.Parse(procT12[1]);
                           
                            disc_furn = oReader.GetDouble(3);
                            if (disc_furn == 0)
                                disc_furn = oReader.GetDouble(4);

                            procDiscFurn = 1 - disc_furn / 100;

                            valArt = oReader.GetDouble(1);

                            if (oReader.GetString(10).Equals("X"))
                                valArt = valArt * ((100 - oReader.GetDouble(11)) / 100);

                            valPoz = valArt - oReader.GetDouble(2) * procDiscFurn - valArt * procT1 / 100 - valArt * procT2 / 100;

                            if (valPoz <= 0)
                                valPoz = valArt - oReader.GetDouble(2) * procDiscFurn - valArt * (procT1 / 2) / 100 - valArt * (procT2 / 2) / 100;

                        }

                        valTotal += valPoz;

                    }
                }



                if (oReader != null)
                {
                    oReader.Close();
                    oReader.Dispose();
                }

                if (cmd != null)
                {
                    cmd.Dispose();
                }

                bazaSalariala.marjaT1 = valTotal;

                if (valTotal == 0)
                    bazaSalariala.procentT1 = 0;
                else
                    bazaSalariala.procentT1 = valTotal / valFTVA;

               

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + cmd.CommandText + " , " + nrCmd + " , " + tipCalcul);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return bazaSalariala;

        }

        public static void getMarjaT1(OracleConnection connection, string idCmd, List<ArticolComandaRap> listArticole)
        {

            
            OracleDataReader oReader = null;
            OracleCommand cmd = null;

            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string lastYear = (cDate.Year - 1).ToString();
            string month = cDate.Month.ToString();


            try
            {
                cmd = connection.CreateCommand();

                

                cmd.CommandText = " select  v.matnr, v.netwr val_art, " +
                                  " nvl(case  " +
                                  " when v.WAVWR = 0 and h.auart = 'ZTP' then(select sum(rfwrt) from sapprd.vbfa f where f.mandt = '900' and f.vbelv = v.vbeln and f.posnv = v.posnr and f.vbtyp_n = 'V') " +
                                  " else WAVWR end, 0) cost_marfa, " +
                                  " nvl((select coef_corr from sapprd.zmarja_coef k where k.mandt = '900' and k.zan = :anCurent and " +
                                  " matnr = d.cod ),0) coef_corr_art, " +
                                  " nvl((select coef_corr from sapprd.zmarja_coef k where k.mandt = '900' and k.zan = :anCurent and " +
                                  " matkl = (select  sintetic from articole where cod = d.cod)  and nvl(length(trim(matnr)), 0)= 0),0) coef_corr_sint, " +
                                  " nvl((select case " +
                                  " when a.t1a_proc + a.t1d_proc > nvl(p.proc_comp, 0) and nvl(p.proc_comp, 0) <> 0 and h.auart = 'ZTP' then a.t1a_proc* p.proc_mics / 100 || '#' || a.t1d_proc * p.proc_mics / 100 " +
                                  " else a.t1a_proc || '#' || a.t1d_proc end procente " +
                                  " from sapprd.ZPROC_T1 a, websap.articole b, sapprd.zdesc_proc p " +
                                  " where a.prctr = v.prctr and a.anv = :an and a.luna = '00' and a.matkl = b.sintetic and a.spart = substr(b.grup_vz, 0, 2) " +
                                  " and b.cod = d.cod and '900' = p.mandt(+) and b.sintetic = p.matkl(+)),'0#0') T12, " +
                                  " h.cod_agent, v.lgort, d.cantitate, " +
                                  " nvl(t.matnr,'-1') art_taxa, nvl(t.ul,'-1') ul_taxa, nvl(t.tip,'-1') tip_taxa, " +
                                  " h.fact_red, d.procent_redb " +
                                  " from sapprd.vbap v, sapprd.zcomhead_tableta h, sapprd.zcomdet_tableta d, sapprd.zarticol_trap t " +
                                  " where v.mandt = '900' and h.mandt = '900' and d.mandt = '900' and h.id =:idCmd and v.vbeln = h.nrcmdsap " +
                                  " and v.matkl not in (select matkl from sapprd.zexc_salarizare where mandt='900' ) " +
                                  " and h.id = d.id and d.cod = v.matnr and t.mandt(+)='900' and t.matnr(+) = d.cod ";



                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":anCurent", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = year;

                cmd.Parameters.Add(":an", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = lastYear;

                cmd.Parameters.Add(":idCmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = Int32.Parse(idCmd);

                oReader = cmd.ExecuteReader();

                double procT1 = 0;
                double procT2 = 0;
                double valPoz = 0;
                double disc_furn = 0;
                double procDiscFurn = 0;
                double cantitate = 0;
                double valArt = 0;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        cantitate = oReader.GetDouble(8);

                        if (!oReader.GetString(9).Equals("-1"))
                        {
                            if (oReader.GetString(10).ToUpper().Equals("BV90") || oReader.GetString(11).ToUpper().Equals("TAXA"))
                                valPoz = 0;
                            else
                            {
                                double valTranspCmd = oReader.GetDouble(1);
                                double valMinTransp = 0;

                                cmd.CommandText = " select nvl(min(x.val_ftva),0) val_min from sapprd.ZTAXA_TRAP x where x.mandt = '900' " +
                                                  " and x.datab <= to_char(sysdate, 'yyyymmdd') and x.datbi >= to_char(sysdate, 'yyyymmdd') and val_ftva <=:valTranspCmd ";

                                cmd.CommandType = System.Data.CommandType.Text;
                                cmd.Parameters.Clear();

                                cmd.Parameters.Add(":valTranspCmd", OracleType.Double, 30).Direction = ParameterDirection.Input;
                                cmd.Parameters[0].Value = valTranspCmd;

                                OracleDataReader oReader1 = cmd.ExecuteReader();

                                if (oReader1.HasRows)
                                {
                                    oReader1.Read();
                                    valMinTransp = oReader1.GetDouble(0);
                                }

                                valPoz = valTranspCmd - valMinTransp;
                                valArt = valPoz;

                                oReader1.Close();

                            }

                        }
                        else
                        {

                            string[] procT12 = oReader.GetString(5).Split('#');

                            procT1 = Double.Parse(procT12[0]);
                            procT2 = Double.Parse(procT12[1]);

                            disc_furn = oReader.GetDouble(3);
                            if (disc_furn == 0)
                                disc_furn = oReader.GetDouble(4);

                            procDiscFurn = 1 - disc_furn / 100;

                            valArt = oReader.GetDouble(1);

                            if (oReader.GetString(12).Equals("X"))
                                valArt = valArt * ((100 - oReader.GetDouble(13)) / 100);

                            valPoz = valArt - oReader.GetDouble(2) * procDiscFurn - valArt * procT1 / 100 - valArt * procT2 / 100;

                            if (valPoz <= 0)
                                valPoz = valArt - oReader.GetDouble(2) * procDiscFurn - valArt * (procT1 / 2) / 100 - valArt * (procT2 / 2) / 100;

                            valPoz = valPoz / cantitate;
                        }

                        foreach (ArticolComandaRap articol in listArticole)
                        {
                            string codArticol = articol.codArticol;

                            if (codArticol.Length == 8)
                                codArticol = "0000000000" + codArticol;

                            if (codArticol.Equals(oReader.GetString(0)))
                            {
                                articol.valT1 = valPoz;

                                if (valArt == 0)
                                    articol.procT1 = 0;
                                else
                                    articol.procT1 = (valPoz / (valArt/cantitate)) * 100;

                            }
                        }

                    }

                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + idCmd + " , " + year + " , " + lastYear);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

        }

        public string getDateNTCF(string codAgent)
        {
            NTCF objNTCF = new NTCF();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            Dictionary<string, int> clientFactAnAnterior = new Dictionary<string, int>();
            Dictionary<string, int> targetAnCurent = new Dictionary<string, int>();
            Dictionary<string, int> clientFactAnCurent = new Dictionary<string, int>();
            Dictionary<string, int> coefAfectare = new Dictionary<string, int>();

            const double coefTarget = 1.15;

            string currentYear = Utils.getCurrentYear();

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string sqlString = " select anv, sum(to_number(IANUARIE)),  sum(to_number(FEBRUARIE)), sum(to_number(MARTIE)), sum(to_number(APRILIE)), " +
                                   " sum(to_number(MAI)), sum(to_number(IUNIE)), sum(to_number(IULIE)), sum(to_number(AUGUST)), sum(to_number(SEPTEMBRIE)), " +
                                   " sum(to_number(OCTOMBRIE)), sum(to_number(NOIEMBRIE)), sum(to_number(DECEMBRIE)) " +
                                   " from sapprd.ZSREAL_NTCF where pernr =:codAgent and anv in (:anCurent, :anAnterior) group by anv ";

                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgent;

                cmd.Parameters.Add(":anCurent", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = currentYear;

                cmd.Parameters.Add(":anAnterior", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = (Int32.Parse(currentYear) - 1).ToString();

                oReader = cmd.ExecuteReader();
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        if (oReader.GetString(0).Equals(currentYear))
                        {
                            clientFactAnCurent.Add("1", oReader.GetInt32(1));
                            clientFactAnCurent.Add("2", oReader.GetInt32(2));
                            clientFactAnCurent.Add("3", oReader.GetInt32(3));
                            clientFactAnCurent.Add("4", oReader.GetInt32(4));
                            clientFactAnCurent.Add("5", oReader.GetInt32(5));
                            clientFactAnCurent.Add("6", oReader.GetInt32(6));
                            clientFactAnCurent.Add("7", oReader.GetInt32(7));
                            clientFactAnCurent.Add("8", oReader.GetInt32(8));
                            clientFactAnCurent.Add("9", oReader.GetInt32(9));
                            clientFactAnCurent.Add("10", oReader.GetInt32(10));
                            clientFactAnCurent.Add("11", oReader.GetInt32(11));
                            clientFactAnCurent.Add("12", oReader.GetInt32(12));
                        }
                        else
                        {

                            targetAnCurent.Add("1", (int)(Math.Round((double.Parse(oReader.GetInt32(1).ToString()) * coefTarget), 0)));
                            targetAnCurent.Add("2", (int)(Math.Round((double.Parse(oReader.GetInt32(2).ToString()) * coefTarget), 0)));
                            targetAnCurent.Add("3", (int)(Math.Round((double.Parse(oReader.GetInt32(3).ToString()) * coefTarget), 0)));
                            targetAnCurent.Add("4", (int)(Math.Round((double.Parse(oReader.GetInt32(4).ToString()) * coefTarget), 0)));
                            targetAnCurent.Add("5", (int)(Math.Round((double.Parse(oReader.GetInt32(5).ToString()) * coefTarget), 0)));
                            targetAnCurent.Add("6", (int)(Math.Round((double.Parse(oReader.GetInt32(6).ToString()) * coefTarget), 0)));
                            targetAnCurent.Add("7", (int)(Math.Round((double.Parse(oReader.GetInt32(7).ToString()) * coefTarget), 0)));
                            targetAnCurent.Add("8", (int)(Math.Round((double.Parse(oReader.GetInt32(8).ToString()) * coefTarget), 0)));
                            targetAnCurent.Add("9", (int)(Math.Round((double.Parse(oReader.GetInt32(9).ToString()) * coefTarget), 0)));
                            targetAnCurent.Add("10", (int)(Math.Round((double.Parse(oReader.GetInt32(10).ToString()) * coefTarget), 0)));
                            targetAnCurent.Add("11", (int)(Math.Round((double.Parse(oReader.GetInt32(11).ToString()) * coefTarget), 0)));
                            targetAnCurent.Add("12", (int)(Math.Round((double.Parse(oReader.GetInt32(12).ToString()) * coefTarget), 0)));

                            clientFactAnAnterior.Add("1", oReader.GetInt32(1));
                            clientFactAnAnterior.Add("2", oReader.GetInt32(2));
                            clientFactAnAnterior.Add("3", oReader.GetInt32(3));
                            clientFactAnAnterior.Add("4", oReader.GetInt32(4));
                            clientFactAnAnterior.Add("5", oReader.GetInt32(5));
                            clientFactAnAnterior.Add("6", oReader.GetInt32(6));
                            clientFactAnAnterior.Add("7", oReader.GetInt32(7));
                            clientFactAnAnterior.Add("8", oReader.GetInt32(8));
                            clientFactAnAnterior.Add("9", oReader.GetInt32(9));
                            clientFactAnAnterior.Add("10", oReader.GetInt32(10));
                            clientFactAnAnterior.Add("11", oReader.GetInt32(11));
                            clientFactAnAnterior.Add("12", oReader.GetInt32(12));


                        }

                    }


                    foreach (KeyValuePair<string, int> entry in clientFactAnCurent)
                    {

                        float cAfect = 0;

                        if (targetAnCurent[entry.Key] > 0)
                            cAfect = (float)entry.Value / (float)targetAnCurent[entry.Key];

                        if (cAfect >= 1)
                            coefAfectare.Add(entry.Key, 10);
                        else
                            coefAfectare.Add(entry.Key, 0);

                    }
                }
                else
                {
                    clientFactAnCurent.Add("1", 0);
                    clientFactAnCurent.Add("2", 0);
                    clientFactAnCurent.Add("3", 0);
                    clientFactAnCurent.Add("4", 0);
                    clientFactAnCurent.Add("5", 0);
                    clientFactAnCurent.Add("6", 0);
                    clientFactAnCurent.Add("7", 0);
                    clientFactAnCurent.Add("8", 0);
                    clientFactAnCurent.Add("9", 0);
                    clientFactAnCurent.Add("10", 0);
                    clientFactAnCurent.Add("11", 0);
                    clientFactAnCurent.Add("12", 0);

                    targetAnCurent.Add("1", 0);
                    targetAnCurent.Add("2", 0);
                    targetAnCurent.Add("3", 0);
                    targetAnCurent.Add("4", 0);
                    targetAnCurent.Add("5", 0);
                    targetAnCurent.Add("6", 0);
                    targetAnCurent.Add("7", 0);
                    targetAnCurent.Add("8", 0);
                    targetAnCurent.Add("9", 0);
                    targetAnCurent.Add("10", 0);
                    targetAnCurent.Add("11", 0);
                    targetAnCurent.Add("12", 0);

                    clientFactAnAnterior.Add("1", 0);
                    clientFactAnAnterior.Add("2", 0);
                    clientFactAnAnterior.Add("3", 0);
                    clientFactAnAnterior.Add("4", 0);
                    clientFactAnAnterior.Add("5", 0);
                    clientFactAnAnterior.Add("6", 0);
                    clientFactAnAnterior.Add("7", 0);
                    clientFactAnAnterior.Add("8", 0);
                    clientFactAnAnterior.Add("9", 0);
                    clientFactAnAnterior.Add("10", 0);
                    clientFactAnAnterior.Add("11", 0);
                    clientFactAnAnterior.Add("12", 0);

                    coefAfectare.Add("1", 0);
                    coefAfectare.Add("2", 0);
                    coefAfectare.Add("3", 0);
                    coefAfectare.Add("4", 0);
                    coefAfectare.Add("5", 0);
                    coefAfectare.Add("6", 0);
                    coefAfectare.Add("7", 0);
                    coefAfectare.Add("8", 0);
                    coefAfectare.Add("9", 0);
                    coefAfectare.Add("10", 0);
                    coefAfectare.Add("11", 0);
                    coefAfectare.Add("12", 0);


                }

                objNTCF.clientFactAnAnterior = clientFactAnAnterior;
                objNTCF.clientFactAnCurent = clientFactAnCurent;
                objNTCF.targetAnCurent = targetAnCurent;
                objNTCF.coefAfectare = coefAfectare;


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , codAgent = " + codAgent);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }


            return new JavaScriptSerializer().Serialize(objNTCF);

        }




    }
}