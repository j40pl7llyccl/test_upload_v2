using System.Windows.Forms;

namespace uIP.MacroProvider.StreamIO.DividedData
{
    public partial class DividedDataForm : Form
    {
        public DividedDataForm()
        {
            InitializeComponent();
        }

        private void Next_Click(object sender, System.EventArgs e)
        {
            //會接到另一個視窗 "python model.py ..."
            //結果會把一些訓練中的train,test,val打印出來,把那些數據抓出來劃出一個趨勢圖
            MessageBox.Show("開始訓練模型......");
        }

        private void bt_Select_Click(object sender, System.EventArgs e)
        {
            // 建立另一個視窗實例
            DataSetSelect dsSelect = new DataSetSelect();

            // 顯示新的視窗
            // 如果你想讓這個新視窗「獨立」顯示，主視窗可繼續操作，就用 Show()
            dsSelect.Show();

            // 如果你想讓這個新視窗屬於「模式」視窗(Modal)，
            // 也就是必須關掉新視窗後才能回到主視窗，則用 ShowDialog()
            // dsForm.ShowDialog();
        }

        private void bt_Auto_Click(object sender, System.EventArgs e)
        {
            DataSetSplitter dsSplitter = new DataSetSplitter();
            dsSplitter.Show();
        }

        private void DividedDataForm_Load(object sender, System.EventArgs e)
        {

        }
    }
}
