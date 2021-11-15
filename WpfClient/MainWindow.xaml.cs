using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace WpfChatClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NetworkStream output; // stream for receiving data           
        private BinaryWriter writer; // facilitates writing to the stream    
        private BinaryReader reader; // facilitates reading from the stream  
        private Thread readThread; // Thread for processing incoming messages
        private string message = "";
        // initialize thread for reading
        public MainWindow()
        {
            InitializeComponent();
            readThread = new Thread(new ThreadStart(RunClient));
            readThread.Start();
        }
        // close all threads associated with this application
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Environment.Exit(System.Environment.ExitCode);
        }
        // sends text the user typed to server
        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return && TxtInput.IsEnabled == true)
                {
                    writer.Write("CLIENT>>> " + TxtInput.Text);
                    TxtDisplay.Text += "\r\nCLIENT>>> " + TxtInput.Text;
                    TxtInput.Clear();
                } // end if
            } // end try
            catch (SocketException)
            {
                TxtDisplay.Text += "\nError writing object";
            } // end catch
        } // end method TxtInput_KeyDown
        public void RunClient()
        {
            TcpClient client=null;

            // instantiate TcpClient for sending data to server
            try
            {
                DisplayMessage("Attempting connection\r\n");

                // Step 1: create TcpClient and connect to server
                client = new TcpClient();
                client.Connect("127.0.0.1", 50000);

                // Step 2: get NetworkStream associated with TcpClient
                output = client.GetStream();

                // create objects for writing and reading across stream
                writer = new BinaryWriter(output);
                reader = new BinaryReader(output);

                DisplayMessage("\r\nGot I/O streams\r\n");
                EnableInput(true); // enable inputTextBox

                // loop until server signals termination
                do
                {
                    // Step 3: processing phase
                    try
                    {
                        // read message from server        
                        message = reader.ReadString();
                        DisplayMessage("\r\n" + message);
                    } // end try
                    catch (Exception)
                    {
                        // handle exception if error in reading server data
                        System.Environment.Exit(System.Environment.ExitCode);
                    } // end catch
                } while (message != "SERVER>>> TERMINATE");

               
            } // end try
            catch (Exception error)
            {
                // handle exception if error in establishing connection
                MessageBox.Show(error.ToString(), "Connection Error",
                   MessageBoxButton.OK, MessageBoxImage.Error);
                
            } // end catch
            finally
            {
                // Step 4: close connection
                writer?.Close();
                reader?.Close();
                output?.Close();
                client?.Close();

                System.Environment.Exit(System.Environment.ExitCode);
            }
        } // end method RunClient
          // method DisplayMessage sets displayTextBox's Text property
          // in a thread-safe manner
        private void DisplayMessage(string message)
        {
            // if modifying displayTextBox is not thread safe
            if (!TxtDisplay.Dispatcher.CheckAccess())
            {
                // use inherited method Invoke to execute DisplayMessage
                // via a delegate                                       
                TxtDisplay.Dispatcher.Invoke(new Action(() => TxtDisplay.Text += message));

            } // end if
            else // OK to modify displayTextBox in current thread
                TxtDisplay.Text += message;
        } // end method DisplayMessage
          // method DisableInput sets inputTextBox's ReadOnly property
          // in a thread-safe manner
        private void EnableInput(bool value)
        {
            // if modifying inputTextBox is not thread safe
            if (!TxtInput.Dispatcher.CheckAccess())
            {
                // use inherited method Invoke to execute DisableInput
                // via a delegate                                     
                TxtInput.Dispatcher.Invoke(new Action(() => TxtInput.IsEnabled = value));
            } // end if
            else // OK to modify inputTextBox in current thread
                TxtInput.IsEnabled = value;
        } // end method DisableInput

        private void TxtInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {

                if (TxtInput.IsEnabled == true)
                {
                    writer.Write("CLIENT>>> " + e.Text);
                    TxtDisplay.Text += "\r\nCLIENT>>> " + e.Text;
                    TxtInput.Clear();
                } // end if
            } // end try
            catch (SocketException)
            {
                TxtDisplay.Text += "\nError writing object";
            } // end catch
        }

        private void TxtInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        } // end method TxtInput_KeyDown


    }
}
