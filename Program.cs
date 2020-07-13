using Dapper;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AccountingModul
{
    class Program
    {
        static void Main(string[] args)
        {
            int customerNumber = 15000000;

            ÇalıştırmaMotoru.KomutÇalıştır("MuhasebeModulu", "MaaşYatır", new object[] { customerNumber });

            ÇalıştırmaMotoru.KomutÇalıştır("MuhasebeModulu", "YıllıkÜcretTahsilEt", new object[] { customerNumber });

            ÇalıştırmaMotoru.BekleyenİşlemleriGerçekleştir();
        }

        //sync examples
        public class ÇalıştırmaMotoru
        {
            public static object[] KomutÇalıştır(string modulsinifAdi, string methodAdi, object[] inputs)
            {
                Assembly asm = typeof(Program).Assembly;
                var type = asm.GetTypes().Where(x => x.Name == modulsinifAdi).FirstOrDefault();
                string methodName = type.GetMethods()[2].Name; //random method

                Task task = new Task(() =>
                {
                    Console.WriteLine(modulsinifAdi + " " + methodAdi + "running");
                    if (methodName == methodAdi)
                        throw new Exception("exception");
                });

                Task succesorTask = task.ContinueWith((antecedentTasks) =>
                    {
                        Veritabanıİşlemleri bussiness = new Veritabanıİşlemleri();
                        //await bussiness.Add<>(); bekleyen iş listesine eklendi.
                        Console.WriteLine(modulsinifAdi + " " + methodAdi + "running");
                    },TaskContinuationOptions.OnlyOnFaulted
               );

                task.Start();
                try
                {
                    task.Wait();
                }
                catch (Exception ex)
                {
                    succesorTask.Wait();
                }

                throw new NotImplementedException();
            }

            public static void BekleyenİşlemleriGerçekleştir()
            {
                Veritabanıİşlemleri bussiness = new Veritabanıİşlemleri();
                //var result = bussiness.GetWaitingList<MuhasebeModulu>(); 
                KomutÇalıştır("result.MuhasebeModulu", "result.YıllıkÜcretTahsilEt", new object[] { "result.customernumber" });
            }
        }

        public class MuhasebeModulu
        {
            public void MaaşYatır(int müşteriNumarası)
            {
                // gerekli işlemler gerçekleştirilir.
                Console.WriteLine(string.Format("{0} numaralı müşterinin maaşı yatırıldı.", müşteriNumarası));
            }

            private void YıllıkÜcretTahsilEt(int müşteriNumarası)
            {
                // gerekli işlemler gerçekleştirilir.
                Console.WriteLine("{0} numaralı müşteriden yıllık kart ücreti tahsil edildi.", müşteriNumarası);
            }

            private void OtomatikÖdemeleriGerçekleştir(int müşteriNumarası)
            {
                // gerekli işlemler gerçekleştirilir.
                Console.WriteLine("{0} numaralı müşterinin otomatik ödemeleri gerçekleştirildi.", müşteriNumarası);
            }
        }

        //async example
        public class Veritabanıİşlemleri
        {
            private static string _connectionString = "<connectionstring>";
            private IDbConnection db { get { return new SqlConnection(_connectionString); } }

            public async Task<T> GetWaitingList<T>(string storedProcedureSuffix = "GetList", object param = null)
            {
                string storedProcuedure = typeof(T).Name.ToString() + storedProcedureSuffix;

                using (db)
                {
                    //get data by Dapper.
                    return await db.QueryFirstOrDefaultAsync<T>(storedProcuedure, param, commandType: CommandType.StoredProcedure);
                }
            }
            public async Task<int> Add<T>(object param = null)
            {
                string storedProcuedure = typeof(T).Name.ToString() + "Add";

                int id = 0;
                using (db)
                {
                    id = await db.ExecuteScalarAsync<int>(storedProcuedure, param, commandType: CommandType.StoredProcedure);
                }
                return id;
            }

        }
    }
}
