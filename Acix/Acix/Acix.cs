using Acix.AcixClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acix
{
    public partial class Acix : Form
    {
        Class1 c = new Class1();
        public DataTable cur_product;//producto actual
        public int indice_aux; //usado en 0.0 y 4.0

        public Acix()
        {
            InitializeComponent();
            Initialize_data_grids();
            groupBox_Resultado.BringToFront();
            groupBox_Equivalencias.BringToFront();
            groupBox_listado_advertencia.BringToFront();
        }

        /****************************************************************************************************************
        *                                             0.0 INICIO LISTADO
        ****************************************************************************************************************/

        private void Initialize_data_grids()
        {
            Initialize_combobox_contenido();
            //**************************************listado de prodcutos**************************************
            DataTable dt = c.Select(@"SELECT dbo.producto.codigo AS Codigo, dbo.producto.marca AS Marca, grado AS Grado, contenido AS Contenido, unidad AS Unidad, stock as Stock, precio_venta AS 'Precio de venta', precio_compra AS 'Precio de compra' 
                                    FROM dbo.producto;");
            dataGridView_listado.DataSource = dt;
            dataGridView_listado.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            //******************************************historial************************************
            DataTable dt_historial = c.Select(@"SELECT codigo AS 'Codigo', dia_hora AS 'Día y hora', descripcion_producto AS 'Descripción del producto', cantidad AS 'Cantidad vendida', ganancia AS 'Ganancia', CASE WHEN vigente = 1 THEN 'SI' ELSE 'NO' END AS Vigente
                                        FROM dbo.historial
                                        Order by dia_hora DESC;");
            dataGridView_historial.DataSource = dt_historial;
            dataGridView_historial.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView_historial.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            //****************************calcular datos de entrada diaria********************************
            //calcular cantidades vendidas y ganancia
            DataTable calculo_diario = c.Select(@"SELECT Sum(ganancia) AS ganancia, SUM (cantidad) AS cantidad
                                                FROM dbo.historial
                                                WHERE vigente = 1 AND CONVERT(DATE, dia_hora) = CONVERT(date, Getdate());");

            if (calculo_diario.Rows.Count > 0)
            {
                label_entrada_diaria_cantidad.Text = calculo_diario.Rows[0]["cantidad"].ToString();
                label_entrada_diaria_total.Text = calculo_diario.Rows[0]["ganancia"].ToString();
            }

            //agregar tabla a la base de datos
            DataTable dt_diaria = c.Select(@"SELECT codigo AS 'Codigo en Historial', dia_hora AS 'Día y hora', descripcion_producto AS 'Descripción del producto', cantidad AS 'Cantidad vendida', ganancia AS 'Ganancia'
                                            FROM dbo.historial
                                            WHERE vigente = 1 AND CONVERT(DATE, dia_hora) = CONVERT(date, Getdate())
                                            Order by dia_hora DESC;");
            dataGridView_entrada_diaria.DataSource = dt_diaria;
            dataGridView_entrada_diaria.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView_entrada_diaria.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            //******************************calcular datos de entrada mensual*******************************
            //calcular cantidades vendidas y ganancia
            DataTable calculo_mensual = c.Select(@"SELECT Sum(ganancia) AS ganancia, SUM (cantidad) AS cantidad
                                                FROM dbo.historial
                                                WHERE vigente = 1 AND MONTH(dia_hora) = MONTH(dateadd(dd, -1, GetDate()))
                                                AND
                                                YEAR(dia_hora) = YEAR(dateadd(dd, -1, GetDate()));");
            if (calculo_mensual.Rows.Count > 0)
            {
                label_entrada_mensual_cantidad.Text = calculo_mensual.Rows[0]["cantidad"].ToString();
                label_entrada_mensual_total.Text = calculo_mensual.Rows[0]["ganancia"].ToString();
            }

            //agregar tabla a la base de datos
            DataTable dt_mensual = c.Select(@"SELECT codigo AS 'Codigo en Historial', dia_hora AS 'Día y hora', descripcion_producto AS 'Descripción del producto', cantidad AS 'Cantidad vendida', ganancia AS 'Ganancia'
                                            FROM dbo.historial
                                            WHERE vigente = 1 AND MONTH(dia_hora) = MONTH(dateadd(dd, -1, GetDate()))
                                            AND
                                            YEAR(dia_hora) = YEAR(dateadd(dd, -1, GetDate()))
                                            Order by dia_hora DESC;");
            dataGridView_entrada_mensual.DataSource = dt_mensual;
            dataGridView_entrada_mensual.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;///
            dataGridView_entrada_mensual.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }

        private void dataGridView_listado_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            indice_aux = e.RowIndex;
            button_listado_eliminar.Enabled = true;
        }

        private void button_listado_eliminar_Click(object sender, EventArgs e)
        {
            groupBox_listado_advertencia.Visible = true;
        }

        private void button_listado_advertencia_si_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dataGridView_listado.Rows[indice_aux];
            string code_to_delete = row.Cells[0].Value.ToString();

            c.Delete("DELETE FROM dbo.Equivalencias WHERE dbo.Equivalencias.codigo1 = " + code_to_delete + "or dbo.Equivalencias.codigo2 = " + code_to_delete + ";");
            c.Delete("DELETE FROM dbo.producto WHERE dbo.producto.codigo = " + code_to_delete + ";");

            Initialize_data_grids();
            button_listado_eliminar.Enabled = false;
            groupBox_listado_advertencia.Visible = false;
            MessageBox.Show("Elemento Borrado");
        }

        private void button_listado_advertencia_no_Click(object sender, EventArgs e)
        {
            groupBox_listado_advertencia.Visible = false;
        }

        /****************************************************************************************************************
        *                                             0.0 FIN LISTADO
        ****************************************************************************************************************/

        /****************************************************************************************************************
        *                                             1.0 INICIO NUEVO
        ****************************************************************************************************************/

        //Inicio Comprobar todos text box llenos
        private void Ask_Fill_Gabs()
        {
            if (textBox_nuevo_marca.Text != "" && textBox_nuevo_grado.Text != "" && textBox_nuevo_contenido.Text != "" && textBox_nuevo_unidad.Text != "" && textBox_nuevo_stock.Text != "" && textBox_nuevo_precioCompra.Text != "" && textBox_nuevo_precio_venta.Text != "")
            {
                button_nuevo_crear.Enabled = true;
            }
        }

        private void textBox_nuevo_marca_TextChanged(object sender, EventArgs e)
        {
            Ask_Fill_Gabs();
        }

        private void textBox_nuevo_grado_TextChanged(object sender, EventArgs e)
        {
            Ask_Fill_Gabs();
        }

        private void textBox_nuevo_contenido_TextChanged(object sender, EventArgs e)
        {
            Ask_Fill_Gabs();
        }

        private void textBox_nuevo_unidad_TextChanged(object sender, EventArgs e)
        {
            Ask_Fill_Gabs();
        }

        private void textBox_nuevo_stock_TextChanged(object sender, EventArgs e)
        {
            Ask_Fill_Gabs();
        }

        private void textBox_nuevo_precio_venta_TextChanged(object sender, EventArgs e)
        {
            Ask_Fill_Gabs();
        }

        private void textBox_nuevo_precioCompra_TextChanged(object sender, EventArgs e)
        {
            Ask_Fill_Gabs();
        }
        //Fin Comprobar todos text box llenos

        //INICIO NUEVOS EQUIVALENTES
        private void textBox_nuevo_codigo_TextChanged(object sender, EventArgs e)
        {
            if(textBox_nuevo_codigo.Text != "")
            {
                button_nuevo_anadir.Enabled = true;
            }
            else
            {
                button_nuevo_anadir.Enabled = false;
            }
        }

        private void button_nuevo_anadir_Click(object sender, EventArgs e)
        {
            string cod = textBox_nuevo_codigo.Text;
            cur_product = c.Select(@"SELECT dbo.producto.codigo AS codigo, CONCAT(dbo.producto.codigo, ' / ' , dbo.producto.marca ,' / ' , grado, ' / ', contenido , ' / ', unidad) AS description 
                                    FROM dbo.producto WHERE dbo.producto.grado = '" + cod + "';");
            
            foreach(DataRow row in cur_product.Rows)
            {
                string descripcion = row["description"].ToString();

                //antes de agregar comprobar que no se agregue un elemento repetido.
                bool repetido = false;

                foreach (String item in listBox_nuevo_equivalentes.Items)
                {
                    if (item == descripcion) repetido = true;
                }
                if (!repetido) listBox_nuevo_equivalentes.Items.Add(descripcion);
            }
            
            //fin antes de agregar comprobar que no se agregue un elemento repetido.

            textBox_nuevo_codigo.Text = "";
        }

        private void button_nuevo_quitar_Click(object sender, EventArgs e)
        {
            listBox_nuevo_equivalentes.Items.Remove( listBox_nuevo_equivalentes.SelectedItem );
            button_nuevo_quitar.Enabled = false;
        }
        //FIN NUEVOS EQUIVALENTES

        private void clear_create_boxes()
        {
            //clear all text boxes
            textBox_nuevo_marca.Text = "";
            textBox_nuevo_grado.Text = "";
            textBox_nuevo_contenido.Text = "";
            textBox_nuevo_unidad.Text = "";
            textBox_nuevo_stock.Text = "";
            textBox_nuevo_precioCompra.Text = "";
            textBox_nuevo_precio_venta.Text = "";

            button_nuevo_crear.Enabled = false;
        }

        private string get_listboxequivalentes_id( string descripcion )   //usado para extraer el codigo de la descripcion de la lista de equivalentes
        {
            string codigo = "";
            foreach (char c in descripcion)
            {
                if (c == ' ') break;
                codigo += c;
            }
            return codigo;
        }
            
        private void button_nuevo_crear_Click(object sender, EventArgs e)
        {
            string marca = textBox_nuevo_marca.Text.ToUpper();
            string grado = textBox_nuevo_grado.Text.ToUpper();
            string contenido = textBox_nuevo_contenido.Text.ToUpper();
            string unidad = textBox_nuevo_unidad.Text.ToUpper();
            string stock = textBox_nuevo_stock.Text.ToUpper();
            string precio_venta = textBox_nuevo_precio_venta.Text.ToUpper();
            string precio_compra = textBox_nuevo_precioCompra.Text.ToUpper();

            

            if (!c.Insert("INSERT INTO producto (marca, grado, contenido, unidad, stock, precio_venta, precio_compra) VALUES ('" + marca + "','" + grado + "','" + contenido + "','" + unidad + "'," + stock + "," + precio_venta + "," + precio_compra + ");"))
            {
                clear_create_boxes();
                MessageBox.Show("Error al insertar producto");
            }
            else MessageBox.Show("Producto añadido exitosamente!");

            //inicio añadir equivalentes 
            string curID = c.curID();

            if (listBox_nuevo_equivalentes.Items.Count > 0)
            {
                foreach (String item in listBox_nuevo_equivalentes.Items)
                {
                    if (!c.Insert("INSERT INTO dbo.Equivalencias (codigo1,codigo2) VALUES (" + curID + "," + get_listboxequivalentes_id(item) + ");"))
                    {
                        clear_create_boxes();
                        MessageBox.Show("Error al insertar los equivalentes del producto");
                        break;
                    }
                }
            }
            //fin añadir equivalentes 

            clear_create_boxes();
            Initialize_data_grids();
            listBox_nuevo_equivalentes.Items.Clear();
        }

        /****************************************************************************************************************
        *                                             1.0 FIN NUEVO
        ****************************************************************************************************************/

        /****************************************************************************************************************
        *                                             2.0 INICIO PEDIDOS
        ****************************************************************************************************************/


        //inicio actualizacion de combo boxes
        public void Initialize_combobox_contenido()
        {
            comboBox_contenido.Items.Clear(); comboBox_contenido.Text = "";
            comboBox_marca.Items.Clear(); comboBox_marca.Text = "";
            comboBox_grado.Items.Clear(); comboBox_grado.Text = "";
            comboBox_unidad.Items.Clear(); comboBox_unidad.Text = "";

            DataTable dt = c.Select("SELECT DISTINCT dbo.producto.contenido FROM dbo.producto;");

            foreach (DataRow row in dt.Rows)
            {
                comboBox_contenido.Items.Add((string)row[0]);
            }
        }

        private void comboBox_contenido_SelectedValueChanged(object sender, EventArgs e)
        {
            comboBox_marca.Items.Clear(); comboBox_marca.Text = "";
            comboBox_grado.Items.Clear(); comboBox_grado.Text = "";
            comboBox_unidad.Items.Clear(); comboBox_unidad.Text = "";

            string contenido = comboBox_contenido.Text;
            DataTable dt = c.Select("SELECT DISTINCT dbo.producto.marca FROM dbo.producto WHERE dbo.producto.contenido LIKE '%" + contenido + "%';");
            foreach (DataRow row in dt.Rows)
            {
                comboBox_marca.Items.Add((string)row[0]);
            }
        }


        private void comboBox_marca_SelectedValueChanged(object sender, EventArgs e)
        {
            comboBox_grado.Items.Clear(); comboBox_grado.Text = "";
            comboBox_unidad.Items.Clear(); comboBox_unidad.Text = "";

            DataTable dt = c.Select(@"SELECT DISTINCT dbo.producto.grado 
                                    FROM dbo.producto
                                    WHERE dbo.producto.marca LIKE '%" + comboBox_marca.Text + "%' AND dbo.producto.contenido LIKE '%" + comboBox_contenido.Text + "%';");
            foreach (DataRow row in dt.Rows)
            {
                comboBox_grado.Items.Add((string)row[0]);
            }

        }

        private void comboBox_grado_SelectedValueChanged(object sender, EventArgs e)
        {
            comboBox_unidad.Items.Clear(); comboBox_unidad.Text = "";

            DataTable dt = c.Select(@"SELECT DISTINCT dbo.producto.unidad 
                                    FROM dbo.producto 
                                    WHERE dbo.producto.marca LIKE '%" + comboBox_marca.Text + "%' AND dbo.producto.grado LIKE '%" + comboBox_grado.Text + "%' AND dbo.producto.contenido LIKE '%" + comboBox_contenido.Text + "%';");
            foreach (DataRow row in dt.Rows)
            {
                comboBox_unidad.Items.Add((string)row[0]);
            }

        }



        //fin actualizacion de combo boxes
       
        private void Btn_descripcion_Click(object sender, EventArgs e)
        {
            DataTable dt = c.Select(@"SELECT dbo.producto.codigo, CONCAT(dbo.producto.codigo, ' / ' , dbo.producto.marca,' / ' , grado, ' / ', contenido , ' / ', unidad) AS description 
                                    FROM dbo.producto
                                    WHERE dbo.producto.marca LIKE '%" + comboBox_marca.Text+"%' AND dbo.producto.grado LIKE '%"+comboBox_grado.Text + "%' AND dbo.producto.contenido LIKE '%"+ comboBox_contenido.Text + "%' AND dbo.producto.unidad LIKE '%" + comboBox_unidad.Text + "%';");
            listBox_Buscar.DataSource = dt;
            listBox_Buscar.ValueMember = "codigo";
            listBox_Buscar.DisplayMember = "description";


            groupBox_Resultado.Visible = true;
            if (listBox_Buscar.Items.Count > 0)
                button_seleccionar_item_list.Enabled = true;
        }

        //start  list search
        private void button1_Click(object sender, EventArgs e)
        {
            DataTable dt = c.Select(@"SELECT dbo.producto.codigo AS Codigo, CONCAT(dbo.producto.codigo, ' / ' , dbo.producto.marca,' / ' , grado, ' / ', contenido , ' / ', unidad) AS description 
                                    FROM dbo.producto
                                    WHERE dbo.producto.codigo=" + textBox_codigo.Text +";");
            listBox_Buscar.DataSource = dt;
            listBox_Buscar.ValueMember = "codigo";
            listBox_Buscar.DisplayMember = "description";

            groupBox_Resultado.Visible = true;
            if (listBox_Buscar.Items.Count > 0)
                button_seleccionar_item_list.Enabled = true;
        }

        private void button_resultado_cerrar_Click(object sender, EventArgs e)
        {
            groupBox_Resultado.Visible = false;
        }
        

        private void button_seleccionar_item_list_Click(object sender, EventArgs e)
        {
            //get Item from list
            DataRowView row = listBox_Buscar.SelectedItem as DataRowView;
            string cod = row["Codigo"].ToString();
            //end

            cur_product = c.Select(@"SELECT dbo.producto.codigo AS Codigo, dbo.producto.stock AS Stock, dbo.producto.precio_venta AS Precio_venta, dbo.producto.precio_compra AS Precio_compra, CONCAT(dbo.producto.codigo, ' / ', dbo.producto.marca, ' / ', grado, ' / ', contenido, ' / ', unidad) AS description 
                                    FROM dbo.producto
                                    WHERE dbo.producto.codigo = " + cod + "; ");
            label_res_descripcion.Text = cur_product.Rows[0]["description"].ToString();
            label_res_stock.Text = cur_product.Rows[0]["Stock"].ToString();
            label_res_pv.Text = cur_product.Rows[0]["Precio_venta"].ToString();

            groupBox_Resultado.Visible = false;
            textBox_cantidad.Enabled = true;
            button_equivalencias.Enabled = true;
        }
        //end list_search

        //Start equivalencias
        private void button_equivalencias_Click(object sender, EventArgs e)
        {
            label_equivalencias_desc.Text = cur_product.Rows[0]["description"].ToString();
            string codigo = cur_product.Rows[0]["codigo"].ToString();

            //fill listbox_equivalencias 
            DataTable dt = c.Select(@"SELECT dbo.producto.codigo AS Codigo, CONCAT(dbo.producto.codigo, ' / ' , dbo.producto.marca,' / ' , grado, ' / ', contenido , ' / ', unidad, ' / ', stock) AS description 
                                    FROM dbo.producto
                                    WHERE dbo.producto.codigo IN(
                                        SELECT dbo.Equivalencias.codigo1 + dbo.Equivalencias.codigo2 - dbo.producto.codigo as equivalentes
                                        FROM dbo.producto
                                        JOIN dbo.Equivalencias ON dbo.producto.codigo = dbo.Equivalencias.codigo1 or dbo.producto.codigo = dbo.Equivalencias.codigo2
                                        WHERE dbo.producto.codigo =" + codigo +");");
            listBox_equivalencias.DataSource = dt;
            listBox_equivalencias.ValueMember = "codigo";
            listBox_equivalencias.DisplayMember = "description";
            //end

            //si la lista es nula no activar el boton select
            if (listBox_equivalencias.Items.Count > 0)
                button_equivalencia_sel.Enabled = true;
            //end

            groupBox_Equivalencias.Visible = true;
        }

        private void button_equivalencia_sel_Click(object sender, EventArgs e)
        {
            //get Item from list
            DataRowView row = listBox_equivalencias.SelectedItem as DataRowView;
            if (row != null)
            {
                string cod = row["Codigo"].ToString();
                //end

                cur_product = c.Select(@"SELECT dbo.producto.codigo AS Codigo, dbo.producto.stock AS Stock, dbo.producto.precio_venta AS Precio_venta, dbo.producto.precio_compra AS Precio_compra, CONCAT(dbo.producto.codigo, ' / ', dbo.producto.marca, ' / ', grado, ' / ', contenido, ' / ', unidad) AS description 
                                        FROM dbo.producto
                                        WHERE dbo.producto.codigo = " + cod + "; ");
                label_res_descripcion.Text = cur_product.Rows[0]["description"].ToString();
                label_res_stock.Text = cur_product.Rows[0]["Stock"].ToString();
                label_res_pv.Text = cur_product.Rows[0]["Precio_venta"].ToString();

                button_equivalencia_sel.Enabled = false;
                groupBox_Equivalencias.Visible = false;
            }
           
        }

        private void button_equivalencia_cerrar_Click(object sender, EventArgs e)
        {
            button_equivalencia_sel.Enabled = false;
            groupBox_Equivalencias.Visible = false;
        }
        //End equivalencias



        private void textBox_cantidad_TextChanged(object sender, EventArgs e)
        {
            if (textBox_cantidad.Text != "")
            {
                try
                {
                    decimal cantidad = decimal.Parse(textBox_cantidad.Text);
                    decimal stock = decimal.Parse(cur_product.Rows[0]["Stock"].ToString());
                    if (cantidad <= stock)
                    {
                        decimal precio = decimal.Parse(cur_product.Rows[0]["Precio_venta"].ToString());
                        decimal resultado = precio * cantidad;
                        label_res_ptotal.Text = resultado.ToString();
                        button_vender.Enabled = true;
                    }
                    else
                    {
                        MessageBox.Show("La cantidad solicitada es mayor que el stock");
                        textBox_cantidad.Text = "";
                    }

                }
                catch (Exception)
                {
                    textBox_cantidad.Text = "";
                    MessageBox.Show("Ingrese datos numéricos");
                }                
            }
            else label_res_ptotal.Text = "0";
        }

        private void clear_pedidos()
        {
            //search_group
            comboBox_marca.Text = "";
            comboBox_grado.Text = "";
            comboBox_contenido.Text = "";
            comboBox_unidad.Text = "";

            //result_group
            label_res_descripcion.Text = "___";
            label_res_stock.Text = "___";
            label_res_pv.Text = "___";
            label_res_ptotal.Text = "0";
            textBox_cantidad.Text = "";

            //searc_by_code
            textBox_codigo.Text = "";

            //clear list
            button_seleccionar_item_list.Enabled = false;
        }


        private void button_vender_Click(object sender, EventArgs e)
        {
            //Inicio Transacciones bd
           
            string descripcion = cur_product.Rows[0]["description"].ToString();//obtener atributos del producto para luego insertar

            decimal cantidad = decimal.Parse( textBox_cantidad.Text );
            
            decimal precio_venta = decimal.Parse(cur_product.Rows[0]["Precio_venta"].ToString());
            decimal precio_compra = decimal.Parse(cur_product.Rows[0]["precio_compra"].ToString());
            decimal ganancia = (precio_venta - precio_compra) * cantidad;

            string fecha_hora = DateTime.Now.ToString("MM-dd-yyyy hh:mm:ss");

            if (c.Insert("INSERT INTO dbo.historial (descripcion_producto, dia_hora, cantidad, ganancia, vigente) VALUES ('"+ descripcion + "','"+ fecha_hora + "',"+ cantidad + ","+ ganancia +",1);"))
            {
                string codigo = cur_product.Rows[0]["codigo"].ToString();
                c.Update("UPDATE dbo.producto SET stock = stock -" + cantidad + "WHERE codigo =" + codigo + ";");
                MessageBox.Show("Venta exitosa!");

                //actualizar dataGridView_historial
                Initialize_data_grids();
            }
            else
            {
                MessageBox.Show("Falló al registrar venta");
            }

            //fin transacciones bd

            clear_pedidos();
            button_equivalencias.Enabled = false;
            textBox_cantidad.Enabled = false;
            button_vender.Enabled = false;
        }

        private void listBox_nuevo_equivalentes_SelectedIndexChanged(object sender, EventArgs e)
        {
            button_nuevo_quitar.Enabled = true;
        }


        /****************************************************************************************************************
         *                                             2.0 FIN PEDIDOS
         ****************************************************************************************************************/

        /****************************************************************************************************************
        *                                             3.0 INICIO ENTRADAS
        ****************************************************************************************************************/
        private void monthCalendar_entrada_diaria_DateChanged(object sender, DateRangeEventArgs e)
        {
            DateTime d = monthCalendar_entrada_diaria.SelectionRange.Start;
            string day = d.Day.ToString();
            string month = d.Month.ToString();
            string year = d.Year.ToString();

            DataTable dt_diaria = c.Select(@"SELECT codigo AS 'Codigo en Historial', dia_hora AS 'Día y hora', descripcion_producto AS 'Descripción del producto', cantidad AS 'Cantidad vendida', ganancia AS 'Ganancia'
                                            FROM dbo.historial
                                            WHERE vigente = 1 AND DAY(dia_hora)= '" + day + @"'
                                            AND
                                            MONTH(dia_hora) = '" + month + @"'
                                            AND
                                            YEAR(dia_hora) = '" + year + @"'
                                            Order by dia_hora DESC;");
            dataGridView_entrada_diaria.DataSource = dt_diaria;
            dataGridView_entrada_diaria.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;///

            //cambiar los labels de cantidad y ganancia
            DataTable calculo_diario = c.Select(@"SELECT Sum(ganancia) AS ganancia, SUM (cantidad) AS cantidad
                                                FROM dbo.historial
                                                WHERE vigente = 1 AND DAY(dia_hora)= '" + day + @"'
                                                AND
                                                MONTH(dia_hora) = '" + month + @"'
                                                AND
                                                YEAR(dia_hora) = '" + year + @"';");

            if (calculo_diario.Rows.Count > 0)
            {
                label_entrada_diaria_cantidad.Text = calculo_diario.Rows[0]["cantidad"].ToString();
                label_entrada_diaria_total.Text = calculo_diario.Rows[0]["ganancia"].ToString();
            }
        }


        private void monthCalendar_entrada_mensual_DateChanged(object sender, DateRangeEventArgs e)
        {
            DateTime d = monthCalendar_entrada_mensual.SelectionRange.Start;
            string month = d.Month.ToString();
            string year = d.Year.ToString();

            DataTable dt_mensual = c.Select(@"SELECT codigo AS 'Codigo en Historial', dia_hora AS 'Día y hora', descripcion_producto AS 'Descripción del producto', cantidad AS 'Cantidad vendida', ganancia AS 'Ganancia'
                                            FROM dbo.historial
                                            WHERE vigente = 1 AND MONTH(dia_hora) = '" + month + @"'
                                            AND
                                            YEAR(dia_hora) = '" + year + @"'
                                            Order by dia_hora DESC;");
            dataGridView_entrada_mensual.DataSource = dt_mensual;
            dataGridView_entrada_mensual.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;///

            //cambiar los labels de cantidad y ganancia

            DataTable calculo_mensual = c.Select(@"SELECT Sum(ganancia) AS ganancia, SUM (cantidad) AS cantidad
                                                FROM dbo.historial
                                                WHERE vigente = 1 AND MONTH(dia_hora) = '" + month + @"'
                                                AND
                                                YEAR(dia_hora) = '" + year + "';");
            if (calculo_mensual.Rows.Count > 0)
            {
                label_entrada_mensual_cantidad.Text = calculo_mensual.Rows[0]["cantidad"].ToString();
                label_entrada_mensual_total.Text = calculo_mensual.Rows[0]["ganancia"].ToString();
            }
        }


        /****************************************************************************************************************
        *                                             3.0 FIN ENTRADAS
        ****************************************************************************************************************/

        /****************************************************************************************************************
         *                                             4.0 INICIO HISTORIAL
         ****************************************************************************************************************/

        private void button_historial_eliminar_Click(object sender, EventArgs e)
        {
            groupBox_historial_advertencia.Visible = true;
        }

        private void dataGridView_historial_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            indice_aux = e.RowIndex;
            button_historial_eliminar.Enabled = true;
        }

        private void button_historial_advertencia_si_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dataGridView_historial.Rows[indice_aux];
            string code_history_to_delete = row.Cells[0].Value.ToString();//codigo
            string descripcion_history_to_delete = row.Cells[2].Value.ToString();//descripcion
            string cantidad = row.Cells[3].Value.ToString();//cantidad

            string codigo_producto = get_listboxequivalentes_id( descripcion_history_to_delete );//line 179

            c.Update("UPDATE dbo.producto SET stock = stock +" + cantidad + "WHERE codigo =" + codigo_producto + ";");
            c.Update("UPDATE dbo.historial SET vigente = 0 WHERE dbo.historial.codigo = " + code_history_to_delete + ";");


            Initialize_data_grids();
            button_historial_eliminar.Enabled = false;
            groupBox_historial_advertencia.Visible = false;
            MessageBox.Show("Elemento del historial borrado");
        }

        private void button_historial_advertencia_no_Click(object sender, EventArgs e)
        {
            groupBox_historial_advertencia.Visible = false;
        }


        /****************************************************************************************************************
         *                                             4.0 FIN HISTORIAL
         ****************************************************************************************************************/

        /****************************************************************************************************************
        *                                             5.0 INICIOAnadir equivalentes
        ****************************************************************************************************************/

        private void button_anadir_equivalentes_selec_Click(object sender, EventArgs e)
        {
            DataTable dt = c.Select(@"SELECT dbo.producto.codigo, CONCAT(dbo.producto.codigo, ' / ' , dbo.producto.marca,' / ' , grado, ' / ', contenido , ' / ', unidad) AS description 
                                    FROM dbo.producto
                                    WHERE dbo.producto.codigo LIKE '%" + textBox_anadir_equivalentes_codigo.Text + "%';");
            label_anadir_equivalentes_desc.Text = dt.Rows[0]["description"].ToString();
        }

        private void textBox_anadir_equivalentes_codigo_TextChanged(object sender, EventArgs e)
        {
            if (textBox_anadir_equivalentes_codigo.Text != "")
            {
                button_anadir_equivalentes_selec.Enabled = true;
            }
            else
            {
                button_anadir_equivalentes_selec.Enabled = false;
            }
        }

        private void textBox_anadir_equivalentes_grado_TextChanged(object sender, EventArgs e)
        {
            if (textBox_anadir_equivalentes_grado.Text != "")
            {
                button_anadir_equivalentes_anadir.Enabled = true;
            }
            else
            {
                button_anadir_equivalentes_anadir.Enabled = false;
            }
        }

        private void button_anadir_equivalentes_anadir_Click(object sender, EventArgs e)
        {
            string cod = textBox_anadir_equivalentes_grado.Text;
            cur_product = c.Select(@"SELECT dbo.producto.codigo AS codigo, CONCAT(dbo.producto.codigo, ' / ' , dbo.producto.marca,' / ' , grado, ' / ', contenido , ' / ', unidad) AS description 
                                    FROM dbo.producto 
                                    WHERE dbo.producto.grado = '" + cod + "';");

            foreach (DataRow row in cur_product.Rows)
            {
                string descripcion = row["description"].ToString();

                //antes de agregar comprobar que no se agregue un elemento repetido.
                bool repetido = false;

                foreach (String item in listBox_anadir_equivalentes.Items)
                {
                    if (item == descripcion) repetido = true;
                }
                if (!repetido) listBox_anadir_equivalentes.Items.Add(descripcion);
            }

            //fin antes de agregar comprobar que no se agregue un elemento repetido.
            if (listBox_anadir_equivalentes.Items.Count > 0)
                button_anadir_equivalentes__crear.Enabled = true;

            textBox_anadir_equivalentes_grado.Text = "";
        }

        private void listBox_anadir_equivalentes_SelectedIndexChanged(object sender, EventArgs e)
        {
            button_anadir_equivalentes_quitar.Enabled = true;
        }

        private void button_anadir_equivalentes_quitar_Click(object sender, EventArgs e)
        {
            listBox_anadir_equivalentes.Items.Remove(listBox_anadir_equivalentes.SelectedItem);
            button_anadir_equivalentes_quitar.Enabled = false;
        }

        private void button_anadir_equivalentes__crear_Click(object sender, EventArgs e)
        {
            string codigo = textBox_anadir_equivalentes_codigo.Text;
            
            if (listBox_anadir_equivalentes.Items.Count > 0)
            {
                foreach (String item in listBox_anadir_equivalentes.Items)
                {
                    if (!c.Insert("INSERT INTO dbo.Equivalencias (codigo1,codigo2) VALUES (" + codigo + "," + get_listboxequivalentes_id(item) + ");"))
                    {
                        textBox_anadir_equivalentes_codigo.Text = "";
                        textBox_anadir_equivalentes_grado.Text = "";
                        label_anadir_equivalentes_desc.Text = "";
                        MessageBox.Show("Error al insertar los equivalentes del producto");
                        break;
                    }
                }
            }
            //fin añadir equivalentes 

            MessageBox.Show("Equivalentes añadidos exitosamente!");

            textBox_anadir_equivalentes_codigo.Text = "";
            textBox_anadir_equivalentes_grado.Text = "";
            label_anadir_equivalentes_desc.Text = "";

            button_anadir_equivalentes__crear.Enabled = false;
            listBox_anadir_equivalentes.Items.Clear();     
        }

        /****************************************************************************************************************
        *                                             5.0 FIN Anadir equivalentes
        ****************************************************************************************************************/




    }
}
