﻿using System;
//using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

/*---------------------------------------
 * --- DE LA TABLA PRODUCTO
 * STOCK ES EL TOTAL DE LOS PRODUCTOS
 * CANTIDAD PARCIAL -> ES LO QUE VA DE LA COMPRA ACTUAL SELECCIONADA
 * 
 * ---DE LA TABLA COMPRA
 * CANTIDAD -> CANTIDAD QUE ENTRA EN LA COMPRA
 * CANTIDAD VIGENTE -> DE ESA CANTIDAD, CUANTO AUN ME QUEDA
 * CANTIDAD ACTUAL -> EL TOTAL DEL STOCK DISPONIBLE, SUMANDO TODAS LAS COMPRAS DE UN PRODUCTO
 * 
 * --------------------------------------*/

namespace Acix.AcixClasses
{
    public class Class1
    {
        static string myconnstrng = ConfigurationManager.ConnectionStrings["connstrng"].ConnectionString;

        //Selecting Data from Database
        public DataTable Select( string query)
        {
            ///Step 1: Database Connection
            SqlConnection conn = new SqlConnection(myconnstrng);
            DataTable dt = new DataTable();
            try
            {
                //Step 2: Writing SQL Query
                string sql = query;
                //Creating cmd using sql and conn
                SqlCommand cmd = new SqlCommand(sql, conn);
                //Creating SQL DataAdapter using cmd
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                conn.Open();
                adapter.Fill(dt);
            }
            catch (Exception ex)
            {
 
            }
            finally
            {
                conn.Close();
            }
            return dt;
        }

        //Selecting Data from Database
        public bool Update(string query)
        {
            ///Step 1: Database Connection
            SqlConnection conn = new SqlConnection(myconnstrng);
            DataTable dt = new DataTable();
            try
            {
                //Step 2: Writing SQL Query
                string sql = query;
                //Creating cmd using sql and conn
                SqlCommand cmd = new SqlCommand(sql, conn);
                //Creating SQL DataAdapter using cmd
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                conn.Open();
                adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                conn.Close();
                return false;
            }
            conn.Close();
            return true;
        }

        public bool Insert(string query)
        {
            ///Step 1: Database Connection
            SqlConnection conn = new SqlConnection(myconnstrng);
            DataTable dt = new DataTable();
            try
            {
                //Step 2: Writing SQL Query
                string sql = query;
                //Creating cmd using sql and conn
                SqlCommand cmd = new SqlCommand(sql, conn);
                //Creating SQL DataAdapter using cmd
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                conn.Open();
                adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                conn.Close();
                return false;
            }
            conn.Close();
            return true;
        }

        public bool Delete(string query)
        {
            ///Step 1: Database Connection
            SqlConnection conn = new SqlConnection(myconnstrng);
            DataTable dt = new DataTable();
            try
            {
                //Step 2: Writing SQL Query
                string sql = query;
                //Creating cmd using sql and conn
                SqlCommand cmd = new SqlCommand(sql, conn);
                //Creating SQL DataAdapter using cmd
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                conn.Open();
                adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                conn.Close();
                return false;
            }
            conn.Close();
            return true;
        }

        public string curID()
        {
            ///Step 1: Database Connection
            SqlConnection conn = new SqlConnection(myconnstrng);
            DataTable dt = new DataTable();
            try
            {
                //Step 2: Writing SQL Query
                string sql = "SELECT MAX(codigo) As cur_ID FROM dbo.producto;";
                //Creating cmd using sql and conn
                SqlCommand cmd = new SqlCommand(sql, conn);
                //Creating SQL DataAdapter using cmd
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                conn.Open();
                adapter.Fill(dt);
            }
            catch (Exception ex)
            {
            }
            conn.Close();
            return dt.Rows[0]["cur_ID"].ToString();
        }
        

    }
}
