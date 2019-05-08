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
            groupBox_proveedor_resultadoProductos.BringToFront();
        }

        /****************************************************************************************************************
        *                                             0.0 INICIO LISTADO
        ****************************************************************************************************************/
        private void Initialize_data_grids()
        {
            Initialize_combobox_contenido();
            Initialize_combobox_comprobantes();
            Initialize_combobox_proveedor_contenidoProducto();
            Initialize_combobox_clientes_apellido();
            Initialize_comboBox_proveedor();

            label_venta_precio_total.Text = "0";
            //**************************************listado de prodcutos**************************************
            DataTable dt = c.Select(@"SELECT dbo.producto.codigo AS Codigo, dbo.producto.marca AS Marca, grado AS Grado, contenido AS Contenido, unidad AS Unidad, stock as Stock, precio_compra AS 'Precio de compra', precio_venta AS 'Precio de venta', proveedor_nombre AS 'Proveedor'
                                    FROM dbo.producto;");
            dataGridView_listado.DataSource = dt;
            for (int i = 0; i < dataGridView_listado.Columns.Count; ++i)
                dataGridView_listado.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView_listado.Columns[0].ReadOnly = true;

            //******************************************historial************************************
            DataTable dt_historial = c.Select(@"SELECT dbo.historial.codigo AS Codigo, dia_hora AS 'Día y hora', dbo.comprobante.nombre AS 'Tipo Comprobante', (CAST(dbo.comprobante.serie AS varchar(100))+ ' N° ' +CAST(dbo.comprobante.numero AS varchar(100))) AS 'Numero de Comprobante',  dbo.historial.nombres_cliente AS Cliente, descripcion_producto AS 'Descripción del producto', cantidad AS 'Cantidad', precio_venta AS 'Precio de venta', ganancia AS 'Ganancia', CASE WHEN vigente = 1 THEN 'SI' ELSE 'NO' END AS Vigente
                                        FROM dbo.historial
                                        LEFT JOIN dbo.comprobante ON dbo.comprobante.cod_historial = dbo.historial.codigo
                                        Order by dia_hora DESC;");
            dataGridView_historial.DataSource = dt_historial;
            for( int i=0; i< dataGridView_historial.Columns.Count; ++i)
                dataGridView_historial.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView_listado.Columns[0].ReadOnly = true;


            //****************************** data grid clientes*******************************
            DataTable dt_clientes = c.Select(@"SELECT codigo AS 'Codigo' , nombre AS 'Nombre', apellidos AS 'Apellidos', telefono AS 'Telefono', marca AS 'Marca', vehiculo AS 'Vehiculo', motor AS 'Motor', tipo_aceite AS 'Tipo de Aceite', tipo_filtro AS 'Tipo de Filtro'
                                        FROM dbo.cliente
                                        Order by apellidos DESC;");
            dataGridView_listado_clientes.DataSource = dt_clientes;

            for (int i = 0; i < dataGridView_listado_clientes.Columns.Count; ++i)
                dataGridView_listado_clientes.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView_listado_clientes.Columns[0].ReadOnly = true;
            dataGridView_listado_clientes.Columns[1].ReadOnly = true;
            dataGridView_listado_clientes.Columns[2].ReadOnly = true;

            //****************************** data grid proveedor*******************************
            DataTable dt_proveedor = c.Select(@"SELECT codigo AS 'Codigo', nombre AS 'Nombre', telefono AS 'Telefono', direccion AS 'Direccion'
                                        FROM dbo.proveedor
                                        Order by nombre DESC;");
            dataGridView_proveedor_listado.DataSource = dt_proveedor;
            for (int i = 0; i < dataGridView_proveedor_listado.Columns.Count; ++i)
                dataGridView_proveedor_listado.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView_proveedor_listado.Columns[0].ReadOnly = true;

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
            DataTable dt_diaria = c.Select(@"SELECT dia_hora AS 'Día y hora', descripcion_producto AS 'Descripción del producto', cantidad AS 'Cantidad vendida', ganancia AS 'Ganancia'
                                            FROM dbo.historial
                                            WHERE vigente = 1 AND CONVERT(DATE, dia_hora) = CONVERT(date, Getdate())
                                            Order by dia_hora DESC;");
            dataGridView_entrada_diaria.DataSource = dt_diaria;
            for (int i = 0; i < dataGridView_entrada_diaria.Columns.Count; ++i)
                dataGridView_entrada_diaria.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;


            //******************************calcular datos de entrada mensual*******************************
            //calcular cantidades vendidas y ganancia
            DataTable calculo_mensual = c.Select(@"SELECT Sum(ganancia) AS ganancia, SUM (cantidad) AS cantidad
                                                FROM dbo.historial
                                                WHERE vigente = 1 AND MONTH(dia_hora) = MONTH(dateadd(dd, -1, GetDate()))
                                                AND
                                                YEAR(dia_hora) = YEAR(dateadd(dd, -1, GetDate()));");

            if (calculo_mensual.Rows[0]["cantidad"].ToString() != "")
            {
                label_entrada_mensual_cantidad.Text = calculo_mensual.Rows[0]["cantidad"].ToString();
                label_entrada_mensual_total.Text = calculo_mensual.Rows[0]["ganancia"].ToString();
            }

            //agregar tabla a la base de datos
            DataTable dt_mensual = c.Select(@"SELECT dia_hora AS 'Día y hora', descripcion_producto AS 'Descripción del producto', cantidad AS 'Cantidad vendida', ganancia AS 'Ganancia'
                                            FROM dbo.historial
                                            WHERE vigente = 1 AND MONTH(dia_hora) = MONTH(dateadd(dd, -1, GetDate()))
                                            AND
                                            YEAR(dia_hora) = YEAR(dateadd(dd, -1, GetDate()))
                                            Order by dia_hora DESC;");
            dataGridView_entrada_mensual.DataSource = dt_mensual;

            for (int i = 0; i < dataGridView_entrada_diaria.Columns.Count; ++i)
                dataGridView_entrada_mensual.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        
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
            textBox_nuevo_stock.Text = "";
            textBox_nuevo_precioCompra.Text = "";
            textBox_nuevo_precio_venta.Text = "";

            button_nuevo_crear.Enabled = false;
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

        //inicio actualizacion nombres clientes
        private void textBox_pedidos_cliente_apellido_TextChanged(object sender, EventArgs e)
        {
            //line 57
            if (textBox_pedidos_cliente_apellido.Text == "") return;

            string apellido_cliente = textBox_pedidos_cliente_apellido.Text.ToString();

            //--------------------------actualizar combo_box_nombre
            comboBox_pedidos_nombre_cliente.Items.Clear(); comboBox_pedidos_nombre_cliente.Text = "";

            DataTable dt = c.Select("SELECT DISTINCT dbo.cliente.nombre FROM dbo.cliente WHERE dbo.cliente.apellidos LIKE '%" + apellido_cliente + "%';");
            foreach (DataRow row in dt.Rows)
            {
                comboBox_pedidos_nombre_cliente.Items.Add((string)row[0]);
            }
        }

        //fin actualizacion nombres clientes


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
            public string precio_venta;
            public string precio_total;
            public string ganancia;
            public data(int pos_, string descripcion_, string cantidad_, string precio_venta_, string precio_total_, string ganancia_)
            {
                pos = pos_;
                descripcion =descripcion_;
                cantidad = cantidad_;
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

        private void button_venta_anadir_Click(object sender, EventArgs e)
        {
            string descripcion = cur_product.Rows[0]["description"].ToString();//obtener atributos del producto para luego insertar
            decimal cantidad = decimal.Parse(textBox_cantidad.Text.ToString());
            decimal precio_venta = decimal.Parse(textBox_pedidos_precio_venta.Text.ToString());
            decimal precio_compra = decimal.Parse(cur_product.Rows[0]["precio_compra"].ToString());
            decimal precio_total = precio_venta * cantidad;
            double ganancia = (double)((precio_venta - precio_compra) * cantidad);

            int pos = listado.Count;
            listado.Add(new data(pos, descripcion, cantidad.ToString(), precio_venta.ToString(), precio_total.ToString(), ganancia.ToString()));
               
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

            comboBox_pedidos_nombre_cliente.Text = "";
            textBox_pedidos_cliente_apellido.Text = "";
        }




        private string get_codigo_cliente()
        {
            string apellido_cliente = textBox_pedidos_cliente_apellido.Text.ToString();
            string nombre_cliente = comboBox_pedidos_nombre_cliente.Text.ToString();

            DataTable cur_client = c.Select(@" SELECT codigo
                                                FROM dbo.cliente
                                                WHERE dbo.cliente.apellidos LIKE '%" + apellido_cliente + "%' and dbo.cliente.nombre LIKE '%" + nombre_cliente + "%'; ");
            if (cur_client.Rows.Count > 0) return cur_client.Rows[0]["codigo"].ToString();
            else return "";
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
            string fecha_hora = DateTime.Now.ToString(dateformat);//mi pc

            string apellido_cliente = textBox_pedidos_cliente_apellido.Text.ToString();
            string nombre_cliente = comboBox_pedidos_nombre_cliente.Text.ToString();
            string codigo_cliente = get_codigo_cliente();
            //-----create client
            if (codigo_cliente == "")
            {
                c.Insert("INSERT INTO dbo.cliente (nombre, apellidos) VALUES ('" + nombre_cliente + "','" + apellido_cliente + "');");
                codigo_cliente = get_codigo_cliente();
            }
            //----- end create client----//



            string tipo_comprobante = comboBox_venta_tipoComprobante.Text.ToString();
            string serie = label_venta_comprobanteSerie.Text.ToString();
            string numero = textBox_venta_num_comprobante.Text.ToString();

            bool venta_exitosa = true;

            for (int i = 0; i < listado.Count; ++i) {
                //---------------
                string descripcion = listado[i].descripcion;
                string cantidad = listado[i].cantidad;
                string precio_venta = listado[i].precio_venta;
                string ganancia = listado[i].ganancia;
                //---------------

                if (c.Insert("INSERT INTO dbo.historial (cliente_codigo, nombres_cliente,descripcion_producto, dia_hora, cantidad, precio_venta, ganancia, vigente) VALUES (" + codigo_cliente + ",'" + apellido_cliente + ", " + nombre_cliente + "','" + descripcion + "','" + fecha_hora + "'," + cantidad + "," + precio_venta + "," + ganancia + ",1);"))
                {
                    //crear nuevo comprobante de pago
                    DataTable dt_hist = c.Select("SELECT MAX(codigo) As cur_ID FROM dbo.historial;");
                    string cod_historial = dt_hist.Rows[0]["cur_ID"].ToString();

                    c.Insert("INSERT INTO dbo.comprobante(nombre, serie, numero, cod_historial) VALUES('" + tipo_comprobante + "', '" + serie + "', " + numero + "," + cod_historial + ") ");

                    //actualizar stock
                    string codigo = get_listboxequivalentes_id(descripcion);
                    c.Update("UPDATE dbo.producto SET stock = stock -" + cantidad + "WHERE codigo =" + codigo + ";");
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
                //MessageBox.Show("INSERT INTO dbo.historial (cliente_codigo, nombres_cliente,descripcion_producto, dia_hora, cantidad, ganancia, vigente) VALUES (" + codigo_cliente + ",'" + apellido_cliente + ", " + nombre_cliente + "','" + descripcion + "','" + fecha_hora + "'," + cantidad + "," + ganancia + ",1);");
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

        /******************************************FUNCIONES CAJA CHICA *********************************/
        
        void Initilize_CajaChica()
        {

            //******************************calcular datos de cajaChica*******************************
            //atualizar cantidad inicial y cantidad actual
            DataTable dt_cajaChica = c.Select(@"SELECT *
                                                FROM dbo.caja_chica
                                                WHERE MONTH(dia_hora) = MONTH(dateadd(dd, -1, GetDate()))
                                                AND
                                                YEAR(dia_hora) = YEAR(dateadd(dd, -1, GetDate()));");
            
            if (dt_cajaChica.Rows.Count == 0)
            {
                string fecha_hora = DateTime.Now.ToString(dateformat);
                c.Insert("INSERT INTO dbo.caja_chica(monto_inicial, monto_actual, gastos ,dia_hora) VALUES(200, 200, 0, '" + fecha_hora + "'); ");

                dt_cajaChica = c.Select(@"SELECT *
                                        FROM dbo.caja_chica
                                        WHERE MONTH(dia_hora) = MONTH(dateadd(dd, -1, GetDate()))
                                        AND
                                        YEAR(dia_hora) = YEAR(dateadd(dd, -1, GetDate()));");
            }
            
            textBox_cajaChica_montoInicial.Text = dt_cajaChica.Rows[0]["monto_inicial"].ToString();
            label_cajaChica_montoActual.Text = dt_cajaChica.Rows[0]["monto_actual"].ToString();
            label_cajaChica_gastos.Text = dt_cajaChica.Rows[0]["gastos"].ToString();
         

            //actualizar data_grid DE COSTOS
            DataTable dt_resumen = c.Select(@"SELECT codigo AS Codigo, dia_hora AS 'Fecha y Hora' , descripcion AS Descripcion, costo AS Costo
                                                FROM dbo.gastos
                                                WHERE MONTH(dia_hora) = MONTH(dateadd(dd, -1, GetDate()))
                                                AND
                                                YEAR(dia_hora) = YEAR(dateadd(dd, -1, GetDate()))
                                                Order by dia_hora DESC;");

            dataGridView_cajachica_resumen.DataSource = dt_resumen;
            

            //actualizar los gastos de la entrada mensual y diaria
            //entrada mensual
            string gastos_ = dt_cajaChica.Rows[0]["gastos"].ToString();

            decimal gastos = decimal.Parse(gastos_);
            decimal ganancia = decimal.Parse(label_entrada_mensual_total.Text.ToString());

            label_entrada_mensual_gastos.Text = gastos_;
            label_entrada_mensual_gananciaTotal.Text = (ganancia - gastos).ToString();
           
            //entrada diaria
            //label_entrada_diaria_gananciaTotal = label_cajaChica_gastos;
            
        }


        private void monthCalendar_cajaChica_DateChanged(object sender, DateRangeEventArgs e)
        {
            DateTime d = monthCalendar_entrada_mensual.SelectionRange.Start;
            string month = d.Month.ToString();
            string year = d.Year.ToString();

            //actualizar labels cajaCHica
            DataTable dt_cajaChica = c.Select(@"SELECT *
                                        FROM dbo.caja_chica
                                        WHERE MONTH(dia_hora) = MONTH(dia_hora) = '" + month + @"'
                                        AND
                                        YEAR(dia_hora) = '" + year + @"'
                                        Order by dia_hora DESC;");

            if (dt_cajaChica.Rows.Count == 0) {
                MessageBox.Show("No hubo caja chica en el mes seleccionado");
                return;
            }

            textBox_cajaChica_montoInicial.Text = dt_cajaChica.Rows[0]["monto_inicial"].ToString();
            label_cajaChica_montoActual.Text = dt_cajaChica.Rows[0]["monto_actual"].ToString();
            label_cajaChica_gastos.Text = dt_cajaChica.Rows[0]["gastos"].ToString();
            
            //actualizar data_grid DE COSTOS
            DataTable dt_resumen = c.Select(@"SELECT codigo AS Codigo, dia_hora AS 'Fecha y Hora' , descripcion AS Descripcion, costo AS Costo
                                    FROM dbo.gastos
                                    WHERE MONTH(dia_hora) = MONTH(dia_hora) = '" + month + @"'
                                    AND
                                    YEAR(dia_hora) = '" + year + @"'
                                    Order by dia_hora DESC;");

            dataGridView_cajachica_resumen.DataSource = dt_resumen;

        }

        private void button_cajaChica_actualizarMontoMensual_Click(object sender, EventArgs e)
        {
            string montoInicial = textBox_cajaChica_montoInicial.Text.ToString();
            DateTime d = monthCalendar_entrada_mensual.SelectionRange.Start;
            string month = d.Month.ToString();
            string year = d.Year.ToString();

            if (c.Update("UPDATE dbo.caja_chica SET monto_actual =" +montoInicial+ " , monto_inicial = " + montoInicial + @" 
                            WHERE MONTH(dia_hora) = '" + month + @"'
                            AND
                            YEAR(dia_hora) = '" + year + @"';"))
            {
                MessageBox.Show("Se actualizó el monto inicial del mes");
            }
            else
            {
                MessageBox.Show("No se registro en los gastos de caja chica");
            }

            label_cajaChica_montoActual.Text = montoInicial;
        }

        private void button_cajaChica_realizar_Click(object sender, EventArgs e)
        {
            string descripcion = textBox_cajaChica_desc.Text.ToString();
            string costo = textBox_cajaChica_costo.Text.ToString();
            string fecha_hora = DateTime.Now.ToString(dateformat);

            if (c.Insert("INSERT INTO dbo.gastos(descripcion, costo, dia_hora) VALUES( '" + descripcion + "', '" + costo + "', '" + fecha_hora + "'); ") ){
                //update gastos caja chica
                DateTime d = monthCalendar_entrada_mensual.SelectionRange.Start;
                string month = d.Month.ToString();
                string year = d.Year.ToString();

                if (c.Update("UPDATE dbo.caja_chica SET gastos = gastos + " + costo + @" 
                            WHERE MONTH(dia_hora) = '" + month + @"'
                            AND
                            YEAR(dia_hora) = '" + year + @"';"))
                {
                    ;
                }
                else
                {
                    MessageBox.Show("No se registro en los gastos de caja chica");
                    return;
                }

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
         *                                             4.0 INICIO HISTORIAL
         ****************************************************************************************************************/

        private void button_historial_eliminar_Click(object sender, EventArgs e)
        {
            //falta progrmar lo que es el comprobante y la relacion entre historial factura
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
            string descripcion_history_to_delete = row.Cells[3].Value.ToString();//descripcion
            string cantidad = row.Cells[4].Value.ToString();//cantidad

            string codigo_producto = get_listboxequivalentes_id(descripcion_history_to_delete);//line 179

            DataTable dt = c.Select("SELECT vigente FROM dbo.historial WHERE dbo.historial.codigo = " + code_history_to_delete + ";");
            if (dt.Rows[0]["vigente"].ToString() == "0")
            {
                MessageBox.Show("No se puede borrar, elemento del historial ya no es vigente");
                return;
            }


            c.Update("UPDATE dbo.producto SET stock = stock +" + cantidad + " WHERE codigo =" + codigo_producto + ";");
            c.Update("UPDATE dbo.historial SET vigente = 0 WHERE dbo.historial.codigo = " + code_history_to_delete + ";");

            //MessageBox.Show("UPDATE dbo.producto SET stock = stock +" + cantidad + " WHERE codigo =" + codigo_producto + ";");
            //MessageBox.Show("UPDATE dbo.historial SET vigente = 0 WHERE dbo.historial.codigo = " + code_history_to_delete + ";");

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
        *                                             3.0 INICIO CLIENTES
        ****************************************************************************************************************/

        private void Initialize_combobox_clientes_apellido()
        {
            //****************************** combo boxes clientes*******************************
            AutoCompleteStringCollection source = new AutoCompleteStringCollection();
            DataTable dt_apellido = c.Select("SELECT DISTINCT dbo.cliente.apellidos FROM dbo.cliente;");
            foreach (DataRow row in dt_apellido.Rows)
            {
                source.Add((string)row[0]);
            }
            textBox_pedidos_cliente_apellido.AutoCompleteCustomSource = source;
            textBox_cliente_historia_apellido.AutoCompleteCustomSource = source;
        }
        
        private void button_cliente_nuevo_crear_Click(object sender, EventArgs e)
        {
            string nombre = textBox_cliente_nuevo_nombre.Text.ToUpper();
            string apellido = textBox_cliente_nuevo_apellido.Text.ToUpper();
            string filtro = textBox_cliente_nuevo_filtro.Text.ToUpper();
            string marca = textBox_cliente_nuevo_marca.Text.ToUpper();
            string motor = textBox_cliente_nuevo_motor.Text.ToUpper();
            string telefono = textBox_cliente_nuevo_telefono.Text.ToUpper();
            string vehiculo = textBox_cliente_nuevo_vehiculo.Text.ToUpper();
            string aceite = textBox_cliente_nuevo_aceite.Text.ToUpper();

            if (!c.Insert("INSERT INTO cliente (nombre, apellidos, telefono, marca, vehiculo, motor, tipo_aceite, tipo_filtro) VALUES ('" + nombre + "','" + apellido + "','" + telefono + "','" + marca + "','" + vehiculo + "','" + motor + "','" + aceite + "','" + filtro + "');"))
            {
                MessageBox.Show("Error al insertar cliente");
            }
            else MessageBox.Show("Cliente añadido exitosamente!");

            Initialize_data_grids();
            textBox_cliente_nuevo_nombre.Text = "";
            textBox_cliente_nuevo_apellido.Text = "";
            textBox_cliente_nuevo_filtro.Text = "";
            textBox_cliente_nuevo_marca.Text = "";
            textBox_cliente_nuevo_motor.Text = "";
            textBox_cliente_nuevo_telefono.Text = "";
            textBox_cliente_nuevo_vehiculo.Text = "";
            textBox_cliente_nuevo_aceite.Text = "";

            Initialize_data_grids();
            button_cliente_nuevo_crear.Enabled = false;
        }
        
        private void textBox_cliente_nuevo_filtro_TextChanged(object sender, EventArgs e)
        {
            button_cliente_nuevo_crear.Enabled = true;
        }
        

        private void button_cliente_historial_selec_Click(object sender, EventArgs e)
        {
            string apellido_cliente = textBox_cliente_historia_apellido.Text.ToString();
            string nombre_cliente = comboBox_cliente_historia_nombre.Text.ToString();

            label_cliente_historia_nombyap.Text = apellido_cliente + ", " + nombre_cliente;

            DataTable dt_clientes = c.Select(@" SELECT dia_hora AS 'Fecha', descripcion_producto AS 'Descripcion del Producto', cantidad AS 'Cantidad'
                                                FROM dbo.cliente 
                                                JOIN dbo.historial ON dbo.cliente.codigo = dbo.historial.cliente_codigo
                                                WHERE vigente = 1 and dbo.cliente.apellidos LIKE '%" + apellido_cliente + "%' and dbo.cliente.nombre LIKE '%" + nombre_cliente + "%' Order by dia_hora DESC;");
            dataGridView_cliente_historial.DataSource = dt_clientes;
            dataGridView_cliente_historial.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            button_cliente_historial_selec.Enabled = false;
        }

        private void textBox_cliente_historia_apellido_TextChanged(object sender, EventArgs e)
        {
            //line 57
            if (textBox_cliente_historia_apellido.Text == "") return;

            string contenido = textBox_cliente_historia_apellido.Text.ToString();
            //--------------------------actualizar combo_box_nombre
            comboBox_cliente_historia_nombre.Items.Clear(); comboBox_cliente_historia_nombre.Text = "";


            DataTable dt = c.Select("SELECT DISTINCT dbo.cliente.nombre FROM dbo.cliente WHERE dbo.cliente.apellidos LIKE '%" + contenido + "%';");
            foreach (DataRow row in dt.Rows)
            {
                comboBox_cliente_historia_nombre.Items.Add((string)row[0]);
            }
        }


        private void comboBox_cliente_historia_nombre_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (textBox_cliente_historia_apellido.Text != "") button_cliente_historial_selec.Enabled = true;
        }

        private void dataGridView_listado_clientes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            indice_aux = e.RowIndex;
            button_cliente_modificar.Enabled = true;
        }

        private void button_cliente_listaddo_advertencia_si_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dataGridView_listado_clientes.Rows[indice_aux];
            string codigo = row.Cells[0].Value.ToString();
            string nombre = row.Cells[1].Value.ToString();
            string appelido = row.Cells[2].Value.ToString();
            string telefono = row.Cells[3].Value.ToString();
            string marca = row.Cells[4].Value.ToString();
            string vehiculo = row.Cells[5].Value.ToString();
            string motor = row.Cells[6].Value.ToString();
            string aceite = row.Cells[7].Value.ToString();
            string filtro = row.Cells[8].Value.ToString();
            //nombre, apellidos, telefono, marca, vehiculo, motor, tipo_aceite, tipo_filtro
            if (c.Update("UPDATE dbo.cliente SET nombre = '" + nombre + "', apellidos = '" + appelido + "',telefono = '" + telefono + "',marca = '" + marca + "',vehiculo = '" + vehiculo + "',motor = '" + motor + "',tipo_aceite = '" + aceite + "',tipo_filtro = '" + filtro + "' WHERE dbo.cliente.codigo = " + codigo + ";"))
            {
                MessageBox.Show("Cliente Actualizado");
            }
            else
            {
                MessageBox.Show("No se pudo actualizar cliente");
            }

            Initialize_data_grids();
            groupBox_clientes_listado_advertencia.Visible = false;
        }

        private void button_cliente_listaddo_advertencia_no_Click(object sender, EventArgs e)
        {
            groupBox_clientes_listado_advertencia.Visible = false;
        }

        private void button_cliente_modificar_Click(object sender, EventArgs e)
        {

            groupBox_clientes_listado_advertencia.Visible = true;
            button_cliente_modificar.Enabled = false;

        }

        /****************************************************************************************************************
        *                                             3.0 FIN CLIENTES
        ****************************************************************************************************************/

        /****************************************************************************************************************
        *                                             6.0 INICIO PROVEEDOR
        ****************************************************************************************************************/

        private void Initialize_comboBox_proveedor()
        {
            DataTable dt = c.Select("SELECT DISTINCT dbo.proveedor.nombre FROM dbo.proveedor;");

            foreach (DataRow row in dt.Rows)
            {
                cbox_proveedor_nombre.Items.Add((string)row[0]);
            }
        }

        private void cbox_proveedor_nombre_SelectedValueChanged(object sender, EventArgs e)
        {
            button_proveedor_selec_proveedor.Enabled = true;
        }

        private void button_proveedor_selec_proveedor_Click(object sender, EventArgs e)
        {
            label_preoveedor_anadir_proveedor.Text = cbox_proveedor_nombre.Text;
            cbox_proveedor_nombre.Text = "";
            button_proveedor_selec_proveedor.Enabled = false;
        }

        //**********************************************INICIO DE BUSCAR PRODUCTO*****************************************************
        //**********************************************INICIO DE BUSCAR PRODUCTO*****************************************************

        //inicio actualizacion de combo boxes
        public void Initialize_combobox_proveedor_contenidoProducto()
        {
            cBox_proveedor_contenido.Items.Clear(); cBox_proveedor_contenido.Text = "";
            cBox_proveedor_marca.Items.Clear(); cBox_proveedor_marca.Text = "";
            cBox_proveedor_grado.Items.Clear(); cBox_proveedor_grado.Text = "";
            cBox_proveedor_Unidad.Items.Clear(); cBox_proveedor_Unidad.Text = "";

            cbox_proveedor_nombre.Items.Clear(); cbox_proveedor_nombre.Text = "";
            DataTable dt = c.Select("SELECT DISTINCT dbo.producto.contenido FROM dbo.producto;");

            foreach (DataRow row in dt.Rows)
            {
                cBox_proveedor_contenido.Items.Add((string)row[0]);
            }
        }

        private void cBox_proveedor_contenido_SelectedValueChanged(object sender, EventArgs e)
        {

            cBox_proveedor_marca.Items.Clear(); cBox_proveedor_marca.Text = "";
            cBox_proveedor_grado.Items.Clear(); cBox_proveedor_grado.Text = "";
            cBox_proveedor_Unidad.Items.Clear(); cBox_proveedor_Unidad.Text = "";

            string contenido = cBox_proveedor_contenido.Text;
            DataTable dt = c.Select("SELECT DISTINCT dbo.producto.marca FROM dbo.producto WHERE dbo.producto.contenido LIKE '%" + contenido + "%';");
            foreach (DataRow row in dt.Rows)
            {
                cBox_proveedor_marca.Items.Add((string)row[0]);
            }
        }

        private void cBox_proveedor_marca_SelectedValueChanged(object sender, EventArgs e)
        {
            cBox_proveedor_grado.Items.Clear(); cBox_proveedor_grado.Text = "";
            cBox_proveedor_Unidad.Items.Clear(); cBox_proveedor_Unidad.Text = "";

            DataTable dt = c.Select(@"SELECT DISTINCT dbo.producto.grado 
                                    FROM dbo.producto
                                    WHERE dbo.producto.marca LIKE '%" + cBox_proveedor_marca.Text + "%' AND dbo.producto.contenido LIKE '%" + cBox_proveedor_contenido.Text + "%';");
            foreach (DataRow row in dt.Rows)
            {
                cBox_proveedor_grado.Items.Add((string)row[0]);
            }
        }

        private void cBox_proveedor_grado_SelectedValueChanged(object sender, EventArgs e)
        {
            cBox_proveedor_Unidad.Items.Clear(); cBox_proveedor_Unidad.Text = "";

            DataTable dt = c.Select(@"SELECT DISTINCT dbo.producto.unidad 
                                    FROM dbo.producto 
                                    WHERE dbo.producto.marca LIKE '%" + cBox_proveedor_marca.Text + "%' AND dbo.producto.grado LIKE '%" + cBox_proveedor_grado.Text + "%' AND dbo.producto.contenido LIKE '%" + cBox_proveedor_contenido.Text + "%';");
            foreach (DataRow row in dt.Rows)
            {
                cBox_proveedor_Unidad.Items.Add((string)row[0]);
            }
        }

        private void button_proveedor_selec_producto_Click(object sender, EventArgs e)
        {
            DataTable dt = c.Select(@"SELECT dbo.producto.codigo, (CAST(dbo.producto.codigo AS varchar(100))+ ' / ' +CAST(dbo.producto.marca AS varchar(100))+' / ' + CAST(dbo.producto.grado AS varchar(100))+ ' / '+ CAST(dbo.producto.contenido AS varchar(100)) + ' / '+ CAST(dbo.producto.unidad AS varchar(100))) AS description 
                                    FROM dbo.producto
                                    WHERE dbo.producto.marca LIKE '%" + cBox_proveedor_marca.Text + "%' AND dbo.producto.grado LIKE '%" + cBox_proveedor_grado.Text + "%' AND dbo.producto.contenido LIKE '%" + cBox_proveedor_contenido.Text + "%' AND dbo.producto.unidad LIKE '%" + cBox_proveedor_Unidad.Text + "%';");
            listBox_proveedor_resultadoProducto.DataSource = dt;
            listBox_proveedor_resultadoProducto.ValueMember = "codigo";
            listBox_proveedor_resultadoProducto.DisplayMember = "description";

            groupBox_proveedor_resultadoProductos.Visible = true;
            if (listBox_proveedor_resultadoProducto.Items.Count > 0)
                button_proveedor_resultado_selec.Enabled = true;
        }

        private void button_proveedor_resultado_selec_Click(object sender, EventArgs e)
        {
            //get Item from list
            DataRowView row = listBox_proveedor_resultadoProducto.SelectedItem as DataRowView;
            string cod = row["Codigo"].ToString();
            //end

            cur_product = c.Select(@"SELECT dbo.producto.codigo AS Codigo, dbo.producto.stock AS Stock, dbo.producto.precio_venta AS Precio_venta, dbo.producto.precio_compra AS Precio_compra, (CAST(dbo.producto.codigo AS varchar(100))+ ' / ' +CAST(dbo.producto.marca AS varchar(100))+' / ' + CAST(dbo.producto.grado AS varchar(100))+ ' / '+ CAST(dbo.producto.contenido AS varchar(100)) + ' / '+ CAST(dbo.producto.unidad AS varchar(100))) AS description 
                                    FROM dbo.producto
                                    WHERE dbo.producto.codigo = " + cod + "; ");

            label_preoveedor_anadir_desc_producto.Text = cur_product.Rows[0]["description"].ToString();

            groupBox_proveedor_resultadoProductos.Visible = false;
            button_proveedor_anadir.Enabled = true;
        }

        private void button_proveedor_resultado_cerrar_Click(object sender, EventArgs e)
        {
            groupBox_proveedor_resultadoProductos.Visible = false;
        }
        //**********************************************FIN DE BUSCAR PRODUCTO*****************************************************
        //**********************************************FIN DE BUSCAR PRODUCTO*****************************************************

        private void button_proveedor_anadir_Click(object sender, EventArgs e)
        {
            string codigo_producto = cur_product.Rows[0]["codigo"].ToString();
            string proveedor_nombre = label_preoveedor_anadir_proveedor.Text.ToString();
            //update
            if (!c.Insert(@"update dbo.producto
                set proveedor_nombre = '" + proveedor_nombre + "'where codigo = " + codigo_producto  + "; "))
            {
                MessageBox.Show("Error al añadir proveedor al producto");
            }
            else
            {
                MessageBox.Show("Proveedor añadido a producto exitosamente!");
            }

            Initialize_data_grids();
            button_proveedor_anadir.Enabled = false;
            label_preoveedor_anadir_desc_producto.Text = "";
            label_preoveedor_anadir_proveedor.Text = "";
        }
        
        //**********************************************NUEVO PROVEDOR*****************************************************

        private void button_proveedor_nuevo_crear_Click(object sender, EventArgs e)
        {
            string proveedor_nombre = textBox_proveedor_nuevo_nombre.Text.ToUpper();
            string proveedor_direccion = textBox_proveedor_nuevo_direccion.Text.ToUpper();
            string proveedor_telefono = textBox_proveedor_nuevo_telefono.Text.ToUpper();
            
            if (!c.Insert("INSERT INTO proveedor(nombre, direccion, telefono) VALUES('" + proveedor_nombre + "', '" + proveedor_direccion + "', '" + proveedor_telefono + "');"))
            {
                MessageBox.Show("Error al crear proveedor");
            }
            else
            {
                MessageBox.Show("Proveedor creado exitosamente!");
            }

            Initialize_data_grids();
            button_proveedor_nuevo_crear.Enabled = false;
            textBox_proveedor_nuevo_nombre.Text = "";
            textBox_proveedor_nuevo_direccion.Text = "";
            textBox_proveedor_nuevo_telefono.Text = "";
        }

        private void textBox_proveedor_nuevo_telefono_TextChanged(object sender, EventArgs e)
        {
            if (textBox_proveedor_nuevo_telefono.Text != "") button_proveedor_nuevo_crear.Enabled = true;
        }

        //**************************************actualizar proveedor

        private void dataGridView_proveedor_listado_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            indice_aux = e.RowIndex;
            button_proveedor_listado_modificar.Enabled = true;
        }


        private void button_proveedor_listado_modificar_Click(object sender, EventArgs e)
        {
            groupBox_proveedor_listado_advertencia.Visible = true;
            button_proveedor_listado_modificar.Enabled = false;
        }

        private void button_proveedor_advertencia_si_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dataGridView_proveedor_listado.Rows[indice_aux];
            string codigo = row.Cells[0].Value.ToString();
            string nombre = row.Cells[1].Value.ToString();
            string telefono = row.Cells[2].Value.ToString();
            string direccion = row.Cells[3].Value.ToString();

            //nombre, direccion, telefono
            if (c.Update("UPDATE dbo.proveedor SET nombre = '" + nombre + "', telefono = '" + telefono + "',direccion = '" + direccion + "' WHERE dbo.proveedor.codigo = " + codigo + ";"))
            {
                MessageBox.Show("Proveedor Actualizado");
            }
            else
            {
                MessageBox.Show("No se pudo actualizar proveedor");
            }

            Initialize_data_grids();
            groupBox_proveedor_listado_advertencia.Visible = false;
        }

        private void button_proveedor_advertencia_no_Click(object sender, EventArgs e)
        {
            groupBox_proveedor_listado_advertencia.Visible = false;
        }

        /****************************************************************************************************************
*                                             6.0 FIN PROVEEDOR
****************************************************************************************************************/
    }
}
