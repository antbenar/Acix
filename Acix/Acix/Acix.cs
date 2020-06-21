using Acix.AcixClasses;
using System;
//using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using DGVPrinterHelper;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;

namespace Acix
{
    public partial class Acix : Form
    {
        Class1 c = new Class1();
        public DataTable cur_product;//producto actual
        public int indice_aux; //usado en 0.0, 4.0 y 3.0(line 900)
        string dateformat;
        BindingList<data> listado;

        public Acix()
        {
            //dateformat = "MM-dd-yyyy hh:mm:ss"; //mi pc
            dateformat = "dd-MM-yyyy hh:mm:ss";
            listado = new BindingList<data>();

            InitializeComponent();
            Initialize_data_grids();
            Initilize_CajaChica();
            groupBox_Resultado.BringToFront();
            groupBox_Equivalencias.BringToFront();
            groupBox_listado_advertencia.BringToFront();
            groupBox_compra_resultado.BringToFront();
            groupBox_kardex.BringToFront();
            /*
            //metodo para agregar codigo a producto en historial
            DataTable dt = c.Select("SELECT descripcion_producto AS descripcion FROM dbo.historial");
            for (int i = 0; i < dt.Rows.Count; ++i)
            {
                string descripcion = dt.Rows[i]["descripcion"].ToString();
                c.Update("UPDATE dbo.historial SET codigo_producto = " + get_listboxequivalentes_id(descripcion) + " WHERE descripcion_producto = '"+ descripcion +"';");
            }
            */
        }

        /****************************************************************************************************************
        *                                             0.0 INICIO LISTADO
        ****************************************************************************************************************/
        private void Initialize_data_grids()
        {
            Initialize_combobox_contenido();
            Initialize_combobox_comprobantes();
            Initialize_combobox_compra_contenido();

            label_venta_precio_total.Text = "0";
            //**************************************listado de prodcutos**************************************
            DataTable dt = c.Select(@"SELECT dbo.producto.codigo AS Codigo, dbo.producto.marca AS Marca, grado AS Grado, contenido AS Contenido, unidad AS Unidad, stock AS 'Cantidad en Stock'
                                    FROM dbo.producto;");
            dataGridView_listado.DataSource = dt;
            for (int i = 0; i < dataGridView_listado.Columns.Count; ++i)
                dataGridView_listado.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView_listado.Columns[0].ReadOnly = true;

            //******************************************historial// venta************************************
            InitializeVenta();

            //****************************************** compra ************************************
             
            DataTable dt_compras = c.Select(@"SELECT dia_hora AS 'Fecha', cantidad AS 'Cantidad', proveedor AS 'Proveedor', descripcion_producto AS Producto, precio_compra AS 'Precio de compra'
                                            FROM dbo.compra
                                            WHERE MONTH(dia_hora) = MONTH(dateadd(dd, -1, GetDate()))
                                            AND
                                            YEAR(dia_hora) = YEAR(dateadd(dd, -1, GetDate()))
                                            Order by dia_hora DESC;");
            dataGridView_compra.DataSource = dt_compras;
            for (int i = 0; i < dataGridView_compra.Columns.Count; ++i)
                dataGridView_compra.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            //****************************calcular datos de entrada diaria********************************
            InitializeEntradaDiaria();

            //******************************calcular datos de entrada mensual*******************************
            InitializeEntradaMensual();


        }

        //Fin inicializar data grids

        private void dataGridView_listado_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            indice_aux = e.RowIndex;
            button_listado_eliminar.Enabled = true;
            button_producto_modificar.Enabled = true;
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

        private void button_producto_modificar_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dataGridView_listado.Rows[indice_aux];
            string codigo = row.Cells[0].Value.ToString();
            string marca = row.Cells[1].Value.ToString();
            string grado = row.Cells[2].Value.ToString();
            string contenido = row.Cells[3].Value.ToString();
            string unidad = row.Cells[4].Value.ToString();
            string stock = row.Cells[5].Value.ToString();
            string precio_venta = row.Cells[6].Value.ToString();
            string precio_compra = row.Cells[7].Value.ToString();
            string proveedor = row.Cells[8].Value.ToString();

            if (c.Update(@" UPDATE dbo.producto
                        SET marca = '" + marca + "', grado = '" + grado + "', contenido = '" + contenido + "', unidad = '" + unidad + @"',
                        stock = " + stock + ", precio_venta = " + precio_venta + ", precio_compra = " + precio_compra + ", proveedor_nombre = '" + proveedor + @"'
                        WHERE codigo =" + codigo + ";"))
            {
                MessageBox.Show("Producto Modificado");
            }
            else MessageBox.Show("No se pudo modificar correctamente el producto");

            Initialize_data_grids();
            button_producto_modificar.Enabled = false;
        }

        private string get_listboxequivalentes_id(string descripcion)   //usado para extraer el codigo de la descripcion de la lista de equivalentes
        {
            string codigo = "";
            foreach (char c in descripcion)
            {
                if (c == ' ' || c == '/') break;
                codigo += c;
            }
            return codigo;
        }

        /****************************************************************************************************************
        *                                             0.0 FIN LISTADO
        ****************************************************************************************************************/

        /****************************************************************************************************************
        *                                             1.0 INICIO NUEVO
        ****************************************************************************************************************/
        //INICIO NUEVOS EQUIVALENTES
        private void textBox_nuevo_codigo_TextChanged(object sender, EventArgs e)
        {
            if (textBox_nuevo_codigo.Text != "")
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
            cur_product = c.Select(@"SELECT dbo.producto.codigo AS codigo, (CAST(dbo.producto.codigo AS varchar(100))+ ' / ' +CAST(dbo.producto.marca AS varchar(100))+' / ' + CAST(dbo.producto.grado AS varchar(100))+ ' / '+ CAST(dbo.producto.contenido AS varchar(100)) + ' / '+ CAST(dbo.producto.unidad AS varchar(100))) AS description 
                                    FROM dbo.producto WHERE dbo.producto.grado = '" + cod + "';");

            foreach (DataRow row in cur_product.Rows)
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
            listBox_nuevo_equivalentes.Items.Remove(listBox_nuevo_equivalentes.SelectedItem);
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

            button_nuevo_crear.Enabled = false;
        }

        

        private void button_nuevo_crear_Click(object sender, EventArgs e)
        {
            string marca = textBox_nuevo_marca.Text.ToUpper();
            string grado = textBox_nuevo_grado.Text.ToUpper();
            string contenido = textBox_nuevo_contenido.Text.ToUpper();
            string unidad = textBox_nuevo_unidad.Text.ToUpper();
            string stock = "0";
            string precio_venta = "0";
            string precio_compra = "0";



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

        public void Initialize_combobox_comprobantes()
        {
            comboBox_venta_tipoComprobante.Items.Clear(); comboBox_venta_tipoComprobante.Text = "";

            DataTable dt = c.Select("SELECT DISTINCT dbo.comprobante.nombre FROM dbo.comprobante;");

            foreach (DataRow row in dt.Rows)
            {
                comboBox_venta_tipoComprobante.Items.Add((string)row[0]);
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
       
       //inicio actualizacion comprobantes de pago
        private void comboBox_venta_tipoComprobante_SelectedValueChanged(object sender, EventArgs e)
        {
            string tipo_comprobante = comboBox_venta_tipoComprobante.Text.ToString();
            DataTable dt = c.Select(@"SELECT serie, numero AS numero 
                                        FROM dbo.comprobante 
                                        WHERE nombre = '" + tipo_comprobante + @"' 
                                        GROUP BY serie, numero ORDER BY numero DESC;;");

            string serie = dt.Rows[0]["serie"].ToString();
            string numero_ = dt.Rows[0]["numero"].ToString();

            int numero = int.Parse(numero_);
            numero = numero + 1;
            numero_ = numero.ToString();
           
            label_venta_comprobanteSerie.Text = serie;
            textBox_venta_num_comprobante.Text = numero_;
        }


        private void Btn_descripcion_Click(object sender, EventArgs e)
        {
            DataTable dt = c.Select(@"SELECT dbo.producto.codigo, (CAST(dbo.producto.codigo AS varchar(100))+ ' / ' +CAST(dbo.producto.marca AS varchar(100))+' / ' + CAST(dbo.producto.grado AS varchar(100))+ ' / '+ CAST(dbo.producto.contenido AS varchar(100)) + ' / '+ CAST(dbo.producto.unidad AS varchar(100))) AS description 
                                    FROM dbo.producto
                                    WHERE dbo.producto.marca LIKE '%" + comboBox_marca.Text + "%' AND dbo.producto.grado LIKE '%" + comboBox_grado.Text + "%' AND dbo.producto.contenido LIKE '%" + comboBox_contenido.Text + "%' AND dbo.producto.unidad LIKE '%" + comboBox_unidad.Text + "%';");
            
            listBox_Buscar.DisplayMember = "description";
            listBox_Buscar.ValueMember = "codigo";
            listBox_Buscar.DataSource = dt;
            listBox_Buscar.BindingContext = this.BindingContext;

            groupBox_Resultado.Visible = true;
        }

        //start  list search
        private void button1_Click(object sender, EventArgs e)
        {
            DataTable dt = c.Select(@"SELECT dbo.producto.codigo, (CAST(dbo.producto.codigo AS varchar(100))+ ' / ' +CAST(dbo.producto.marca AS varchar(100))+' / ' + CAST(dbo.producto.grado AS varchar(100))+ ' / '+ CAST(dbo.producto.contenido AS varchar(100)) + ' / '+ CAST(dbo.producto.unidad AS varchar(100))) AS description 
                                    FROM dbo.producto
                                    WHERE dbo.producto.codigo=" + textBox_codigo.Text + ";");

            listBox_Buscar.DisplayMember = "description";
            listBox_Buscar.ValueMember = "codigo";
            listBox_Buscar.DataSource = dt;
            listBox_Buscar.BindingContext = this.BindingContext;

            groupBox_Resultado.Visible = true;
        }


        //----------------------------begin operaciones listbox------------------------
        class data
        {
            public int pos;
            public string general;
            public string descripcion;
            public string cantidad;
            public string cantidad_actual;
            public string precio_venta;
            public string precio_total;
            public string ganancia;
            public data(int pos_, string descripcion_, string cantidad_, string cantidad_actual_, string precio_venta_, string precio_total_, string ganancia_)
            {
                pos = pos_;
                descripcion =descripcion_;
                cantidad = cantidad_;
                cantidad_actual = cantidad_actual_;
                precio_venta = precio_venta_;
                precio_total = precio_total_;
                ganancia = ganancia_;
                general = descripcion + " -> " + precio_venta + " -> " + cantidad + " -> " + precio_total;
            }

            public string Descripcion { get { return general; } }
            public int Pos { get { return pos; } }
        };

        public void clear_anadirPedidos()
        {
            //result_group
            label_res_descripcion.Text = "___";
            label_res_stock.Text = "___";
            textBox_pedidos_precio_venta.Text = "___";
            label_res_ptotal.Text = "0";
            textBox_cantidad.Text = "";
        }

        private double obtenerGanancia(string cod_producto, decimal cantidad) //calcular la ganancia consultando todos los precios del producto en stock disponibles
        {
            decimal precio_venta = decimal.Parse(textBox_pedidos_precio_venta.Text.ToString());
            
            DataTable dt_prodcuto = c.Select("SELECT * FROM dbo.producto WHERE codigo = " + cod_producto + ";");
            decimal cantidad_actual = decimal.Parse(dt_prodcuto.Rows[0]["cantidad_parcial"].ToString());
            decimal precio_compra = decimal.Parse(dt_prodcuto.Rows[0]["precio_compra"].ToString());

            if (cantidad <= cantidad_actual)
            {
                return (double)((precio_venta - precio_compra) * cantidad);//ganancia
            }

            double ganancia = (double)((precio_venta - precio_compra) * cantidad_actual);//venta parcial(con todo lo que hay hasta ese numero de compra
            cantidad = cantidad - cantidad_actual;

            DataTable dt = c.Select("SELECT * FROM dbo.compra WHERE cantidad_vigente > 0 AND codigo_producto = " + cod_producto + " ORDER BY codigo;");

            for (int i = 0; i < dt.Rows.Count; ++i)
            {
                decimal cantidad_compra_actual = decimal.Parse(dt.Rows[i]["cantidad_vigente"].ToString());
                precio_compra = decimal.Parse(dt.Rows[i]["precio_compra"].ToString()); ;

                if (cantidad < cantidad_compra_actual)
                {
                    ganancia += (double)((precio_venta - precio_compra) * cantidad);
                    return ganancia;
                }
                else
                {
                    ganancia += (double)((precio_venta - precio_compra) * cantidad_compra_actual);
                    cantidad = cantidad - cantidad_compra_actual;
                }
            }

            return ganancia;
        }

        private void button_venta_anadir_Click(object sender, EventArgs e)
        {
            string descripcion = cur_product.Rows[0]["description"].ToString();//obtener atributos del producto para luego insertar
            decimal cantidad = decimal.Parse(textBox_cantidad.Text.ToString());
            decimal precio_venta = decimal.Parse(textBox_pedidos_precio_venta.Text.ToString());
            decimal precio_compra = decimal.Parse(cur_product.Rows[0]["precio_compra"].ToString());
            decimal precio_total = precio_venta * cantidad;
            double ganancia = obtenerGanancia(get_listboxequivalentes_id(descripcion), cantidad);
            
            DataTable dt_prodcuto = c.Select("SELECT stock FROM dbo.producto WHERE codigo = " + get_listboxequivalentes_id(descripcion) + ";");
            decimal cantidad_actual = decimal.Parse(dt_prodcuto.Rows[0]["stock"].ToString()) - cantidad;

            int pos = listado.Count;
            listado.Add(new data(pos, descripcion, cantidad.ToString(), cantidad_actual.ToString() ,precio_venta.ToString(), precio_total.ToString(), ganancia.ToString()));
               
            //actualizar listbox 
            listBox_venta_listado.DataSource = listado;
            listBox_venta_listado.DisplayMember = "Descripcion";
            listBox_venta_listado.ValueMember = "Pos";
            listBox_venta_listado.BindingContext = this.BindingContext;
            listBox_venta_listado.Refresh();

            //actualizar precio total de todos los productos en lista
            decimal sum = decimal.Parse(label_venta_precio_total.Text.ToString());
            sum += precio_total;
            label_venta_precio_total.Text = sum.ToString();

            clear_anadirPedidos();
            groupBox_Resultado.Visible = false;
            button_venta_anadir.Enabled = false;
        }

        private void listBox_venta_listado_SelectedIndexChanged(object sender, EventArgs e)
        {
            button_venta_quitarLista.Enabled = true;
        }

        private void button_resultado_cerrar_Click(object sender, EventArgs e)
        {
            groupBox_Resultado.Visible = false;
            clear_anadirPedidos();
        }

        //----strar quitar elemento de listbox
            public void remove_listado( int pos )
            {
                
                MessageBox.Show(listado[pos].pos.ToString());
                listado.RemoveAt(pos);
                for (int i = pos; i < listado.Count; ++i)
                    listado[i].pos -= 1;
            }

        private void button_venta_quitarLista_Click(object sender, EventArgs e)
        {
            int pos = int.Parse(listBox_venta_listado.SelectedValue.ToString());
            MessageBox.Show(pos.ToString() + " " + listado.Count.ToString() );
            //actualizar precio total de todos los productos en lista
            decimal sum = decimal.Parse(label_venta_precio_total.Text.ToString());
            sum -= decimal.Parse(listado[pos].precio_total);
            label_venta_precio_total.Text = sum.ToString();

            remove_listado(pos); // remove item from listado
            listBox_venta_listado.DataSource = listado;
            listBox_venta_listado.DisplayMember = "Descripcion";
            listBox_venta_listado.ValueMember = "Pos";
            listBox_venta_listado.BindingContext = this.BindingContext;
            listBox_venta_listado.Refresh();

            button_venta_quitarLista.Enabled = false;
        }

        //----end quitar elemento de listbox

        private void listBox_Buscar_SelectedIndexChanged(object sender, EventArgs e)
        {
            //get Item from list
            DataRowView row = listBox_Buscar.SelectedItem as DataRowView;
            string cod = row["Codigo"].ToString();
            //end

            cur_product = c.Select(@"SELECT dbo.producto.codigo AS Codigo, dbo.producto.stock AS Stock, dbo.producto.precio_venta AS Precio_venta, dbo.producto.precio_compra AS Precio_compra, (CAST(dbo.producto.codigo AS varchar(100))+ ' / ' +CAST(dbo.producto.marca AS varchar(100))+' / ' + CAST(dbo.producto.grado AS varchar(100))+ ' / '+ CAST(dbo.producto.contenido AS varchar(100)) + ' / '+ CAST(dbo.producto.unidad AS varchar(100))) AS description 
                                    FROM dbo.producto
                                    WHERE dbo.producto.codigo = " + cod + "; ");
            label_res_descripcion.Text = cur_product.Rows[0]["description"].ToString();
            label_res_stock.Text = cur_product.Rows[0]["Stock"].ToString();
            textBox_pedidos_precio_venta.Text = cur_product.Rows[0]["Precio_venta"].ToString();

            textBox_cantidad.Enabled = true;
            textBox_pedidos_precio_venta.Enabled = true;
            button_equivalencias.Enabled = true;
        }


        //-----end operaciones listbox

        //Start equivalencias
        private void button_equivalencias_Click(object sender, EventArgs e)
        {
            label_equivalencias_desc.Text = cur_product.Rows[0]["description"].ToString();
            string codigo = cur_product.Rows[0]["codigo"].ToString();

            //fill listbox_equivalencias 
            DataTable dt = c.Select(@"SELECT dbo.producto.codigo AS Codigo, (CAST(dbo.producto.codigo AS varchar(100))+ ' / ' +CAST(dbo.producto.marca AS varchar(100))+' / ' + CAST(dbo.producto.grado AS varchar(100))+ ' / '+ CAST(dbo.producto.contenido AS varchar(100)) + ' / '+ CAST(dbo.producto.unidad AS varchar(100))) AS description 
                                    FROM dbo.producto
                                    WHERE dbo.producto.codigo IN(
                                        SELECT dbo.Equivalencias.codigo1 + dbo.Equivalencias.codigo2 - dbo.producto.codigo as equivalentes
                                        FROM dbo.producto
                                        JOIN dbo.Equivalencias ON dbo.producto.codigo = dbo.Equivalencias.codigo1 or dbo.producto.codigo = dbo.Equivalencias.codigo2
                                        WHERE dbo.producto.codigo =" + codigo + ");");
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

        private void button_equivalencia_sel_Click_1(object sender, EventArgs e)
        {
            //get Item from list
            DataRowView row = listBox_equivalencias.SelectedItem as DataRowView;
            if (row != null)
            {
                string cod = row["Codigo"].ToString();
                //end

                cur_product = c.Select(@"SELECT dbo.producto.codigo AS Codigo, dbo.producto.stock AS Stock, dbo.producto.precio_venta AS Precio_venta, dbo.producto.precio_compra AS Precio_compra, (CAST(dbo.producto.codigo AS varchar(100))+ ' / ' +CAST(dbo.producto.marca AS varchar(100))+' / ' + CAST(dbo.producto.grado AS varchar(100))+ ' / '+ CAST(dbo.producto.contenido AS varchar(100)) + ' / '+ CAST(dbo.producto.unidad AS varchar(100))) AS description 
                                        FROM dbo.producto
                                        WHERE dbo.producto.codigo = " + cod + "; ");
                label_res_descripcion.Text = cur_product.Rows[0]["description"].ToString();
                label_res_stock.Text = cur_product.Rows[0]["Stock"].ToString();
                textBox_pedidos_precio_venta.Text = cur_product.Rows[0]["Precio_venta"].ToString();
            }
            button_equivalencia_sel.Enabled = false;
            groupBox_Equivalencias.Visible = false;
        }

        private void button_equivalencia_cerrar_Click_1(object sender, EventArgs e)
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
                        decimal precio = decimal.Parse(textBox_pedidos_precio_venta.Text.ToString());
                        decimal resultado = precio * cantidad;
                        label_res_ptotal.Text = resultado.ToString();
                        button_venta_anadir.Enabled = true;
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

        private void textBox_pedidos_precio_venta_TextChanged(object sender, EventArgs e)
        {
            if (textBox_pedidos_precio_venta.Text != "")
            {
                try
                {
                    if (textBox_cantidad.Text != "")
                    {
                        decimal cantidad = decimal.Parse(textBox_cantidad.Text);
                        decimal precio = decimal.Parse(textBox_pedidos_precio_venta.Text.ToString());
                        decimal resultado = precio * cantidad;
                        label_res_ptotal.Text = resultado.ToString();
                    }
                }
                catch (Exception)
                {
                    textBox_pedidos_precio_venta.Text = "";
                    //MessageBox.Show("Ingrese datos numéricos");
                }
            }
        }

        private void clear_pedidos()
        {
            //search_group
            comboBox_marca.Text = "";
            comboBox_grado.Text = "";
            comboBox_contenido.Text = "";
            comboBox_unidad.Text = "";

            //comprobantes group
            label_venta_comprobanteSerie.Text = "";
            textBox_venta_num_comprobante.Text = "";
            comboBox_venta_tipoComprobante.Text = "";

            //searc_by_code
            textBox_codigo.Text = "";

            //clear listbox venta_listado
            listado.Clear();
            listBox_venta_listado.DataSource=listado;

            textBox_pedidos_cliente_apellido.Text = "";
        }

        private void actualizarCantidadVigenteCompra(string cod_producto, decimal cantidad_) //calcular la ganancia consultando todos los precios del producto en stock disponibles
        {

            DataTable dt_prodcuto = c.Select("SELECT * FROM dbo.producto WHERE codigo = " + cod_producto + ";");
            decimal cantidad_actual = decimal.Parse(dt_prodcuto.Rows[0]["cantidad_parcial"].ToString());
            decimal cantidad = cantidad_;

            if (cantidad <= cantidad_actual)        //lo que ya está cargado en producto
            {
                c.Update("UPDATE dbo.producto SET stock = stock - " + cantidad + ", cantidad_parcial = cantidad_parcial - " + cantidad + " WHERE codigo = " + cod_producto + ";");
                return;
            }

            cantidad = cantidad - cantidad_actual;

            DataTable dt = c.Select("SELECT * FROM dbo.compra WHERE cantidad_vigente > 0 AND codigo_producto = " + cod_producto + " ORDER BY codigo;");

            for (int i = 0; i < dt.Rows.Count; ++i)
            {
                decimal cantidad_compra_actual = decimal.Parse(dt.Rows[i]["cantidad_vigente"].ToString());
                string codigo_compra = dt.Rows[i]["codigo"].ToString();

                if (cantidad < cantidad_compra_actual)
                {
                    string precio_compra = dt.Rows[i]["precio_compra"].ToString();
                    string cantidad_vigente = dt.Rows[i]["cantidad_vigente"].ToString();

                    //MessageBox.Show("pc = " + precio_compra + ", cv = " + cantidad_vigente + ", ca = " + cantidad);

                    c.Update("UPDATE dbo.compra SET cantidad_vigente = 0   WHERE codigo = " + codigo_compra + ";");
                    c.Update("UPDATE dbo.producto SET stock = stock - " + cantidad_ + ", precio_compra = " + precio_compra + " , cantidad_parcial = " + cantidad_vigente + " - " + cantidad + " WHERE codigo = " + cod_producto + ";");
                    return;
                }
                else
                {
                    c.Update("UPDATE dbo.compra SET cantidad_vigente = 0 WHERE codigo = " + codigo_compra + ";");
                    cantidad = cantidad - cantidad_compra_actual;
                }
            }
        }

        private string GetPrecioTotal(string cod_producto, decimal cantidad_) //calcular la ganancia consultando todos los precios del producto en stock disponibles
        {

            DataTable dt_prodcuto = c.Select("SELECT * FROM dbo.producto WHERE codigo = " + cod_producto + ";");
            decimal cantidad_actual = decimal.Parse(dt_prodcuto.Rows[0]["cantidad_parcial"].ToString());
            decimal precioCompraTotal = 0;//----------------
            string result = "";

            decimal cantidad = cantidad_;

            if (cantidad <= cantidad_actual)        //lo que ya está cargado en producto
            {
                result = cantidad.ToString() + " * " + dt_prodcuto.Rows[0]["precio_compra"].ToString();//--------------
                return result;
            }
            else
            {
                result = cantidad_actual.ToString() + " * " + dt_prodcuto.Rows[0]["precio_compra"].ToString() + " + "; 
                cantidad = cantidad - cantidad_actual;
            }

            DataTable dt = c.Select("SELECT * FROM dbo.compra WHERE cantidad_vigente > 0 AND codigo_producto = " + cod_producto + " ORDER BY codigo;");

            for (int i = 0; i < dt.Rows.Count; ++i)
            {
                string precio_compra = dt.Rows[i]["precio_compra"].ToString();
                decimal cantidad_compra_actual = decimal.Parse(dt.Rows[i]["cantidad_vigente"].ToString());

                if (cantidad < cantidad_compra_actual)
                {
                    result += cantidad.ToString() + " * " + precio_compra;//-------------
                    precioCompraTotal += decimal.Parse(precio_compra) * cantidad;//--------------
                    //MessageBox.Show(precioCompraTotal.ToString());
                    return result;
                }
                else
                {
                    result += cantidad_compra_actual.ToString() + " * " + precio_compra + " + ";//-------------
                    precioCompraTotal += decimal.Parse(precio_compra) * cantidad_compra_actual;//--------------
                    cantidad = cantidad - cantidad_compra_actual;
                }
            }
            return result;//no sirve
        }


        private void button_vender_Click(object sender, EventArgs e)
        {
            //Inicio Transacciones bd

            if (comboBox_venta_tipoComprobante.Text == "")
            {
                MessageBox.Show("Falta el tipo de factura");
                return;
            }

            //---datos generales de la generacion de la venta
            DateTime d = venta_calendar.SelectionRange.Start;
            TimeSpan ts = DateTime.Now.TimeOfDay;
            d = d.Date + ts;
            string fecha_hora = d.ToString(dateformat);//se cambia el formato de la fecha

            string nombre_cliente = textBox_pedidos_cliente_apellido.Text.ToString();

            string tipo_comprobante = comboBox_venta_tipoComprobante.Text.ToString();
            string serie = label_venta_comprobanteSerie.Text.ToString();
            string numero = textBox_venta_num_comprobante.Text.ToString();

            bool venta_exitosa = true;

            for (int i = 0; i < listado.Count; ++i) {
                //---------------
                string descripcion = listado[i].descripcion;
                string cantidad = listado[i].cantidad;
                string cantidad_actual = listado[i].cantidad_actual;
                string precio_venta = listado[i].precio_venta;
                string ganancia = listado[i].ganancia;

                string codigo = get_listboxequivalentes_id(descripcion);
                string detallesCompra = GetPrecioTotal(codigo, decimal.Parse(cantidad));
                
                //---------------
                
                if (c.Insert("INSERT INTO dbo.historial (nombres_cliente,descripcion_producto, dia_hora, cantidad, precio_venta, ganancia, codigo_producto, cantidad_actual, detalles_compra) VALUES ('" + nombre_cliente + "','" + descripcion + "','" + fecha_hora + "'," + cantidad + "," + precio_venta + "," + ganancia + ","+ get_listboxequivalentes_id(descripcion) +"," + cantidad_actual + ", '" + detallesCompra + "');"))
                {
                    //crear nuevo comprobante de pago
                    DataTable dt_hist = c.Select("SELECT MAX(codigo) As cur_ID FROM dbo.historial;");
                    string cod_historial = dt_hist.Rows[0]["cur_ID"].ToString();

                    c.Insert("INSERT INTO dbo.comprobante(nombre, serie, numero, cod_historial) VALUES('" + tipo_comprobante + "', '" + serie + "', " + numero + "," + cod_historial + ") ");

                    //actualizar stock
                    actualizarCantidadVigenteCompra(codigo, decimal.Parse(cantidad));
                }
                else
                {
                    venta_exitosa = false;
                    break;
                }
            }


            if (venta_exitosa)
            {
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
            textBox_pedidos_precio_venta.Enabled = true;
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
        void InitializeEntradaDiaria()
        {
            DateTime d = monthCalendar_entrada_diaria.SelectionRange.Start;
            string day = d.Day.ToString();
            string month = d.Month.ToString();
            string year = d.Year.ToString();

            DataTable dt_diaria = c.Select(@"SELECT dia_hora AS 'Día y hora', descripcion_producto AS 'Descripción del producto', cantidad AS 'Cantidad vendida', ganancia AS 'Ganancia'
                                            FROM dbo.historial
                                            WHERE DAY(dia_hora)= '" + day + @"'
                                            AND
                                            MONTH(dia_hora) = '" + month + @"'
                                            AND
                                            YEAR(dia_hora) = '" + year + @"'
                                            Order by dia_hora DESC;");
            dataGridView_entrada_diaria.DataSource = dt_diaria;
            for (int i = 0; i < dataGridView_entrada_diaria.Columns.Count; ++i)
                dataGridView_entrada_diaria.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            //cambiar los labels de cantidad y ganancia
            DataTable calculo_diario = c.Select(@"SELECT Sum(ganancia) AS ganancia, SUM (cantidad) AS cantidad
                                                FROM dbo.historial
                                                WHERE DAY(dia_hora)= '" + day + @"'
                                                AND
                                                MONTH(dia_hora) = '" + month + @"'
                                                AND
                                                YEAR(dia_hora) = '" + year + @"';");

            if (calculo_diario.Rows.Count > 0)
            {
                label_entrada_diaria_cantidad.Text = calculo_diario.Rows[0]["cantidad"].ToString();
                label_entrada_diaria_total.Text = calculo_diario.Rows[0]["ganancia"].ToString();
            }
            else
            {
                label_entrada_diaria_cantidad.Text = "0.00";
                label_entrada_diaria_total.Text = "0.00";
            }
        }

        void InitializeEntradaMensual()
        {
            DateTime d = monthCalendar_entrada_mensual.SelectionRange.Start;
            string month = d.Month.ToString();
            string year = d.Year.ToString();

            DataTable dt_mensual = c.Select(@"SELECT dia_hora AS 'Día y hora', descripcion_producto AS 'Descripción del producto', cantidad AS 'Cantidad vendida', ganancia AS 'Entrada', null AS 'Salida'
                                                FROM dbo.historial
                                                WHERE MONTH(dia_hora) = '" + month + @"' AND
                                                YEAR(dia_hora) = '" + year + @"'
                                                UNION
                                                SELECT dia_hora AS 'Día y hora', descripcion AS 'Descripción del producto', null AS 'Cantidad vendida', null AS 'Entrada', costo AS 'Salida'
                                                FROM dbo.gastos
                                                WHERE MONTH(dia_hora) = '" + month + @"' AND
                                                YEAR(dia_hora) = '" + year + @"'
                                                Order by dia_hora;");

            dataGridView_entrada_mensual.DataSource = dt_mensual;
            for (int i = 0; i < dataGridView_entrada_mensual.Columns.Count; ++i)
                dataGridView_entrada_mensual.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            //cambiar los labels de cantidad y ganancia

            DataTable calculo_mensual = c.Select(@"SELECT Sum(ganancia) AS ganancia, SUM (cantidad) AS cantidad
                                                FROM dbo.historial
                                                WHERE MONTH(dia_hora) = '" + month + @"'
                                                AND
                                                YEAR(dia_hora) = '" + year + "';");

            if (calculo_mensual.Rows.Count > 0 && calculo_mensual.Rows[0]["cantidad"].ToString() != "")
            {
                label_entrada_mensual_cantidad.Text = calculo_mensual.Rows[0]["cantidad"].ToString();
                label_entrada_mensual_total.Text = calculo_mensual.Rows[0]["ganancia"].ToString();

                //------------------/actualizar los gastos de la entrada mensual
                DataTable calculo_mensual_gastos = c.Select(@"SELECT Sum(costo) AS gastos
                                                                FROM dbo.gastos
                                                                WHERE MONTH(dia_hora) = '" + month + @"'
                                                                AND
                                                                YEAR(dia_hora) = '" + year + "';");

                if (calculo_mensual_gastos.Rows.Count > 0 && calculo_mensual_gastos.Rows[0]["gastos"].ToString() != "")
                {


                    string gastos_ = calculo_mensual_gastos.Rows[0]["gastos"].ToString();

                    decimal gastos = decimal.Parse(gastos_);
                    decimal ganancia = decimal.Parse(label_entrada_mensual_total.Text.ToString());

                    label_entrada_mensual_gastos.Text = gastos_;
                    label_entrada_mensual_gananciaTotal.Text = (ganancia - gastos).ToString();
                }
                else
                {
                    label_entrada_mensual_gastos.Text = "0.00";
                    label_entrada_mensual_gananciaTotal.Text = label_entrada_mensual_total.Text.ToString();
                }
            }
            else
            {
                label_entrada_mensual_cantidad.Text = "0";
                label_entrada_mensual_total.Text = "0.0";
                label_entrada_mensual_gananciaTotal.Text = "0.0";
            }
        }

        private void monthCalendar_entrada_diaria_DateChanged(object sender, DateRangeEventArgs e)
        {
            InitializeEntradaDiaria();
        }

        private void monthCalendar_entrada_mensual_DateChanged(object sender, DateRangeEventArgs e)
        {
            InitializeEntradaMensual();
        }

        /******************************************FUNCIONES CAJA CHICA *********************************/
        
        void Initilize_CajaChica()
        {
            DateTime d = monthCalendar_cajaChica.SelectionRange.Start;
            string month = d.Month.ToString();
            string year = d.Year.ToString();

            DataTable dt_resumen = c.Select(@"SELECT codigo AS Codigo, dia_hora AS 'Fecha y Hora' , descripcion AS Descripcion, costo AS Costo
                                    FROM dbo.gastos
                                    WHERE MONTH(dia_hora) = '" + month + @"'
                                    AND
                                    YEAR(dia_hora) = '" + year + @"'
                                    Order by dia_hora DESC;");

            dataGridView_cajachica_resumen.DataSource = dt_resumen;
            for (int i = 0; i < dataGridView_cajachica_resumen.Columns.Count; ++i)
                dataGridView_cajachica_resumen.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataTable calculo_mensual_gastos = c.Select(@"SELECT Sum(costo) AS gastos
                                                                FROM dbo.gastos
                                                                WHERE MONTH(dia_hora) = '" + month + @"'
                                                                AND
                                                                YEAR(dia_hora) = '" + year + "';");
            label_cajaChica_gastos.Text = calculo_mensual_gastos.Rows[0]["gastos"].ToString();
        }


        private void monthCalendar_cajaChica_DateChanged(object sender, DateRangeEventArgs e)
        {
            Initilize_CajaChica();
        }

        private void button_cajaChica_realizar_Click(object sender, EventArgs e)
        {
            string descripcion = textBox_cajaChica_desc.Text.ToString();
            string costo = textBox_cajaChica_costo.Text.ToString();

            DateTime dt = monthCalendar_cajaChica.SelectionRange.Start;
            string fecha_hora = dt.ToString(dateformat);

            if (c.Insert("INSERT INTO dbo.gastos(descripcion, costo, dia_hora) VALUES( '" + descripcion + "', '" + costo + "', '" + fecha_hora + "'); ") ){
                //update gastos caja chica
                DateTime d = monthCalendar_entrada_mensual.SelectionRange.Start;
                string month = d.Month.ToString();
                string year = d.Year.ToString();

                MessageBox.Show("Se registró el gasto");
                Initilize_CajaChica();
            }
            else MessageBox.Show("No se pudo registrar el gasto");

            textBox_cajaChica_desc.Text = "";
            textBox_cajaChica_costo.Text = "";
        }



        //******************************************************IMPRIMIR - PRINT **********************************************
        private void button_entrada_mensual_imprimir_Click(object sender, EventArgs e)
        {
            DGVPrinter printer = new DGVPrinter();

            printer.Title = "Reporte Mensual";
            printer.SubTitle = string.Format("{0}", DateTime.Now.Date);
            printer.SubTitleFormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoClip;

            printer.PageNumbers = true;
            printer.PageNumberInHeader = false;
            printer.PorportionalColumns = true;
            printer.HeaderCellAlignment = StringAlignment.Near;
            printer.Footer = "Acix";
            printer.FooterSpacing = 15;

            printer.PrintDataGridView(dataGridView_entrada_diaria);
        }

        private void button_entrad_mensual_imprimi_Click(object sender, EventArgs e)
        {
            DGVPrinter printer = new DGVPrinter();

            printer.Title = "Reporte Mensual";
            printer.SubTitle = string.Format("{0}", DateTime.Now.Date);
            printer.SubTitleFormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoClip;

            printer.PageNumbers = true;
            printer.PageNumberInHeader = false;
            printer.PorportionalColumns = true;
            printer.HeaderCellAlignment = StringAlignment.Near;
            printer.Footer = "Acix";
            printer.FooterSpacing = 15;

            printer.PrintDataGridView(dataGridView_entrada_mensual);
        }


        /****************************************************************************************************************
        *                                             3.0 FIN ENTRADAS
        ****************************************************************************************************************/

        /****************************************************************************************************************
         *                                             4.0 INICIO HISTORIAL// NUEVO VENTA
         ****************************************************************************************************************/
        private void InitializeVenta()
        {
            DateTime d = monthCalendar_venta.SelectionRange.Start;
            string day = d.Day.ToString();
            string month = d.Month.ToString();
            string year = d.Year.ToString();

            DataTable dt_ventas = c.Select(@"SELECT dia_hora AS 'Día y hora', cantidad AS 'Cantidad', descripcion_producto AS 'Descripción del producto', precio_venta AS 'Precio de venta', dbo.comprobante.nombre AS 'Tipo Comprobante', (CAST(dbo.comprobante.serie AS varchar(100))+ ' N° ' +CAST(dbo.comprobante.numero AS varchar(100))) AS 'Numero de Comprobante', dbo.historial.nombres_cliente AS Cliente, detalles_compra AS 'Precios de compra'
                                            FROM dbo.historial
                                            LEFT JOIN dbo.comprobante ON dbo.comprobante.cod_historial = dbo.historial.codigo
                                            WHERE MONTH(dia_hora) = '" + month + @"'
                                            AND
                                            YEAR(dia_hora) = '" + year + @"'
                                            Order by dia_hora DESC;");
            dataGridView_historial.DataSource = dt_ventas;

            for (int i = 0; i < dataGridView_historial.Columns.Count; ++i)
                dataGridView_historial.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }


        private void monthCalendar_venta_DateChanged(object sender, DateRangeEventArgs e)
        {
            InitializeVenta();
        }

        private void button_venta_diaria_Click(object sender, EventArgs e)
        {
            DateTime d = monthCalendar_venta.SelectionRange.Start;
            string day = d.Day.ToString();
            string month = d.Month.ToString();
            string year = d.Year.ToString();

            DataTable dt_ventas = c.Select(@"SELECT dia_hora AS 'Día y hora', cantidad AS 'Cantidad', descripcion_producto AS 'Descripción del producto', precio_venta AS 'Precio de venta', dbo.comprobante.nombre AS 'Tipo Comprobante', (CAST(dbo.comprobante.serie AS varchar(100))+ ' N° ' +CAST(dbo.comprobante.numero AS varchar(100))) AS 'Numero de Comprobante', dbo.historial.nombres_cliente AS Cliente, detalles_compra AS 'Precios de compra'
                                            FROM dbo.historial
                                            LEFT JOIN dbo.comprobante ON dbo.comprobante.cod_historial = dbo.historial.codigo
                                            WHERE DAY(dia_hora)= '" + day + @"'
                                            AND
                                            MONTH(dia_hora) = '" + month + @"'
                                            AND
                                            YEAR(dia_hora) = '" + year + @"'
                                            Order by dia_hora DESC;");
            dataGridView_historial.DataSource = dt_ventas;

            for (int i = 0; i < dataGridView_historial.Columns.Count; ++i)
                dataGridView_historial.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }


        private void dataGridView_historial_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            indice_aux = e.RowIndex;
            button_historial_modificar.Enabled = true;
        }

        private void button_historial_modificar_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dataGridView_historial.Rows[indice_aux];
            string codigo = row.Cells[0].Value.ToString();//codigo
            string date = row.Cells[1].Value.ToString();//date


            if (c.Update(@" UPDATE dbo.historial
                        SET dia_hora = '" + date + @"'
                        WHERE codigo =" + codigo + ";"))
            {
                MessageBox.Show("Fecha Modificada");
            }
            else MessageBox.Show("No se pudo modificar correctamente la fecha");

            Initialize_data_grids();
            button_historial_modificar.Enabled = false;
        }


        /****************************************************************************************************************
         *                                             4.0 FIN HISTORIAL/  VENTA
         ****************************************************************************************************************/

        /****************************************************************************************************************
        *                                             5.0 INICIOAnadir equivalentes
        ****************************************************************************************************************/

        private void button_anadir_equivalentes_selec_Click(object sender, EventArgs e)
        {
            DataTable dt = c.Select(@"SELECT (CAST(dbo.producto.codigo AS varchar(100))+ ' / ' +CAST(dbo.producto.marca AS varchar(100))+' / ' + CAST(dbo.producto.grado AS varchar(100))+ ' / '+ CAST(dbo.producto.contenido AS varchar(100)) + ' / '+ CAST(dbo.producto.unidad AS varchar(100))) AS description 
                                    FROM dbo.producto
                                    WHERE dbo.producto.codigo LIKE '%" + textBox_anadir_equivalentes_codigo.Text.ToString() + "%';");
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
            cur_product = c.Select(@"SELECT dbo.producto.codigo AS codigo, (CAST(dbo.producto.codigo AS varchar(100))+ ' / ' +CAST(dbo.producto.marca AS varchar(100))+' / ' + CAST(dbo.producto.grado AS varchar(100))+ ' / '+ CAST(dbo.producto.contenido AS varchar(100)) + ' / '+ CAST(dbo.producto.unidad AS varchar(100))) AS description 
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

        /****************************************************************************************************************
        *                                             6.0 INICIO COMPRA
        ****************************************************************************************************************/

        //inicio actualizacion de combo boxes
        public void Initialize_combobox_compra_contenido()
        {
            //INICIO inicializar combobox
            comboBox_compra_contenido.Items.Clear(); comboBox_compra_contenido.Text = "";
            comboBox_compra_marca.Items.Clear(); comboBox_compra_marca.Text = "";
            comboBox_compra_grado.Items.Clear(); comboBox_compra_grado.Text = "";
            comboBox_compra_unidad.Items.Clear(); comboBox_compra_unidad.Text = "";

            DataTable dt = c.Select("SELECT DISTINCT dbo.producto.contenido FROM dbo.producto;");

            foreach (DataRow row in dt.Rows)
            {
                comboBox_compra_contenido.Items.Add((string)row[0]);
            }
            ///-----FIN COMBOBOX-----------
        }
     
        private void comboBox_compra_contenido_SelectedValueChanged(object sender, EventArgs e)
        {
            comboBox_compra_marca.Items.Clear(); comboBox_compra_marca.Text = "";
            comboBox_compra_grado.Items.Clear(); comboBox_compra_grado.Text = "";
            comboBox_compra_unidad.Items.Clear(); comboBox_compra_unidad.Text = "";

            string contenido = comboBox_compra_contenido.Text;
            DataTable dt = c.Select("SELECT DISTINCT dbo.producto.marca FROM dbo.producto WHERE dbo.producto.contenido LIKE '%" + contenido + "%';");
            foreach (DataRow row in dt.Rows)
            {
                comboBox_compra_marca.Items.Add((string)row[0]);
            }
        }
    

        private void comboBox_compra_marca_SelectedValueChanged(object sender, EventArgs e)
        {
            comboBox_compra_grado.Items.Clear(); comboBox_compra_grado.Text = "";
            comboBox_compra_unidad.Items.Clear(); comboBox_compra_unidad.Text = "";

            string marca = comboBox_compra_marca.Text;
            string contenido = comboBox_compra_contenido.Text;

            DataTable dt = c.Select(@"SELECT DISTINCT dbo.producto.grado 
                                        FROM dbo.producto
                                        WHERE dbo.producto.marca LIKE '%" + marca + "%' AND dbo.producto.contenido LIKE '%" + contenido + "%';");
            foreach (DataRow row in dt.Rows)
            {
                comboBox_compra_grado.Items.Add((string)row[0]);
            }
        }

        private void comboBox_compra_grado_SelectedValueChanged(object sender, EventArgs e)
        {
            comboBox_compra_unidad.Items.Clear(); comboBox_compra_unidad.Text = "";

            string grado = comboBox_compra_grado.Text;
            string marca = comboBox_compra_marca.Text;
            string contenido = comboBox_compra_contenido.Text;

            DataTable dt = c.Select(@"SELECT DISTINCT dbo.producto.unidad 
                                    FROM dbo.producto 
                                    WHERE dbo.producto.marca LIKE '%" + marca + "%' AND dbo.producto.grado LIKE '%" + grado + "%' AND dbo.producto.contenido LIKE '%" + contenido + "%';");
            foreach (DataRow row in dt.Rows)
            {
                comboBox_compra_unidad.Items.Add((string)row[0]);
            }
        }

        //fin actualizacion de combo boxes


        private void button_compra_seleccionar_Click(object sender, EventArgs e)
        {
            //get Item from list
            DataRowView row = listBox_compra.SelectedItem as DataRowView;
            string cod = row["codigo"].ToString();
            string descripcion = row["description"].ToString();
            //end

            label_compra_producto.Text = descripcion;

            button_compra_seleccionar.Enabled = false;
            groupBox_compra_resultado.Visible = false;
        }

        private void button_compra_cerrar_Click(object sender, EventArgs e)
        {
            button_compra_seleccionar.Enabled = false;
            groupBox_compra_resultado.Visible = false;
        }

        private void button_compra_buscar_Click(object sender, EventArgs e)
        {
            DataTable dt = c.Select(@"SELECT dbo.producto.codigo, (CAST(dbo.producto.codigo AS varchar(100))+ ' / ' +CAST(dbo.producto.marca AS varchar(100))+' / ' + CAST(dbo.producto.grado AS varchar(100))+ ' / '+ CAST(dbo.producto.contenido AS varchar(100)) + ' / '+ CAST(dbo.producto.unidad AS varchar(100))) AS description 
                                    FROM dbo.producto
                                    WHERE dbo.producto.marca LIKE '%" + comboBox_compra_marca.Text + "%' AND dbo.producto.grado LIKE '%" + comboBox_compra_grado.Text + "%' AND dbo.producto.contenido LIKE '%" + comboBox_compra_contenido.Text + "%' AND dbo.producto.unidad LIKE '%" + comboBox_compra_unidad.Text + "%';");

            listBox_compra.DisplayMember = "description";
            listBox_compra.ValueMember = "codigo";
            listBox_compra.DataSource = dt;
            listBox_compra.BindingContext = this.BindingContext;

            groupBox_compra_resultado.Visible = true;
        }

        private void listBox_compra_SelectedIndexChanged(object sender, EventArgs e)
        {
            button_compra_seleccionar.Enabled = true;
        }

        private void clear_pedidos_compra()
        {
            //search_group
            comboBox_compra_marca.Text = "";
            comboBox_compra_grado.Text = "";
            comboBox_compra_contenido.Text = "";
            comboBox_compra_unidad.Text = "";

            textBox_compra_proveedor.Text = "";
            textBox_compra_cantidad.Text = "";
            textBox_compra_precio.Text = "";

            label_compra_producto.Text = "___";
        }

        private void button_compra_comprar_Click(object sender, EventArgs e)
        {
            if (textBox_compra_proveedor.Text == "")
            {
                MessageBox.Show("Falta ingresar el proveedor");
                return; 
            }

            if (textBox_compra_cantidad.Text == "")
            {
                MessageBox.Show("Falta ingresar la cantidad");
                return;
            }

            if(textBox_compra_precio.Text == "")
            {
                MessageBox.Show("Falta ingresar el precio de compra");
                return;
            }

            //---datos generales de la generacion de la venta
            DateTime d = compra_calendar.SelectionRange.Start;
            TimeSpan ts = DateTime.Now.TimeOfDay;
            d = d.Date + ts;
            string fecha_hora = d.ToString(dateformat);//se cambia el formato de la fecha

            string desc = label_compra_producto.Text.ToString();
            string codigo = get_listboxequivalentes_id(desc);

            string proveedor = textBox_compra_proveedor.Text.ToString();
            string cantidad = textBox_compra_cantidad.Text.ToString();
            string precio = textBox_compra_precio.Text.ToString();

            //Inicio Transacciones bd
            c.Update("UPDATE dbo.producto SET stock = stock +" + cantidad + " WHERE codigo =" + codigo + ";");
            string stock = c.Select("SELECT stock FROM dbo.producto WHERE codigo =" + codigo + ";").Rows[0]["stock"].ToString();

            if (c.Insert("INSERT INTO dbo.compra (descripcion_producto, proveedor, dia_hora, cantidad, cantidad_vigente, precio_compra, codigo_producto, cantidad_actual) VALUES ('" + desc + "','" + proveedor + "','" + fecha_hora + "'," + cantidad + "," + cantidad + "," + precio + "," + codigo + "," + stock + ");"))
            {
                MessageBox.Show("Compra exitosa!");
                //actualizar dataGridView_historial
                Initialize_data_grids();
            }
            else
            {
                MessageBox.Show("Falló al registrar compra");
            }
            //fin transacciones bd

            clear_pedidos_compra();
        }

        /****************************************************************************************************************
        *                                             6.0 FIN COMPRA
        ****************************************************************************************************************/

        /****************************************************************************************************************
        *                                             7.0 INICIO KARDEX
        ****************************************************************************************************************/

        private void button_kardex_buscar_Click(object sender, EventArgs e)
        {
             string desc = textBox_kardex_buscar.Text.ToString();

            DataTable dt = c.Select(@"SELECT dbo.producto.codigo, (CAST(dbo.producto.codigo AS varchar(100))+ ' / ' +CAST(dbo.producto.marca AS varchar(100))+' / ' + CAST(dbo.producto.grado AS varchar(100))+ ' / '+ CAST(dbo.producto.contenido AS varchar(100)) + ' / '+ CAST(dbo.producto.unidad AS varchar(100))) AS description 
                                    FROM dbo.producto
                                    WHERE dbo.producto.marca LIKE '%" + desc + "%' OR dbo.producto.grado LIKE '%" + desc + "%' OR dbo.producto.contenido LIKE '%" + desc + "%' OR dbo.producto.unidad LIKE '%" + desc  + "%';");

            listBox_kardex_resultado.DisplayMember = "description";
            listBox_kardex_resultado.ValueMember = "codigo";
            listBox_kardex_resultado.DataSource = dt;
            listBox_kardex_resultado.BindingContext = this.BindingContext;

            groupBox_kardex.Visible = true;
        }

        private void button_kardex_selec_Click(object sender, EventArgs e)
        {
            //get Item from list
            DataRowView row = listBox_kardex_resultado.SelectedItem as DataRowView;
            string cod = row["codigo"].ToString();
            string descripcion = row["description"].ToString();
            //end

            label_kardex_producto.Text = descripcion;

            //inicio actualizar datagrid
            DataTable dt = c.Select(@"SELECT dia_hora AS Fecha, descripcion_producto AS Producto, cantidad AS Entrada, null AS Salida, cantidad_actual AS Saldo 
                                    FROM dbo.compra
                                    WHERE codigo_producto = " + cod + @"
                                    UNION ALL
                                    SELECT dia_hora AS Fecha, descripcion_producto AS Producto, null AS Entrada, cantidad AS Salida, cantidad_actual AS Saldo 
                                    FROM dbo.historial
                                    WHERE codigo_producto = " + cod + @"
                                    ORDER BY dia_hora; ");
            dataGridView_kardex.DataSource = dt;
            for (int i = 0; i < dataGridView_compra.Columns.Count; ++i)
                dataGridView_kardex.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            //fin actualizar data grid



            button_kardex_selec.Enabled = false;
            groupBox_kardex.Visible = false;
        }

        private void button_kardex_cerrar_Click(object sender, EventArgs e)
        {
            button_kardex_selec.Enabled = false;
            groupBox_kardex.Visible = false;
        }

        private void listBox_kardex_resultado_SelectedIndexChanged(object sender, EventArgs e)
        {
            button_kardex_selec.Enabled = true;
        }




        /****************************************************************************************************************
        *                                             7.0 FIN KARDEX
        ****************************************************************************************************************/

    }
}
