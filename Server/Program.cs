using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

public class ChatServer
{
    const int port = 4040;
    TcpListener server;
    List<StreamWriter> clients = new List<StreamWriter>();
    object locker = new object();
    int MaxClient = 1;
    public ChatServer()
    {
        server = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
    }

    public void Start()
    {
        server.Start();
        Console.WriteLine("Server started on port " + port);

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("New client connected");
            Task.Run(() => HandleClient(client));
        }
    }

    private void HandleClient(TcpClient client)
    {
        try
        {
            NetworkStream ns = client.GetStream();
            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns) { AutoFlush = true };

            lock (locker)
            {
                if (clients.Count >= MaxClient) {
                    Console.WriteLine("Max clients in chat!");
                    sw.WriteLine($"---------------------- don't can connect ----------------------");
                    client.Close();
                    MaxClient--;
                    return;
                }
                clients.Add(sw);
            }

            string? message;
            while ((message = sr.ReadLine()) != null)
            {
                if (message == "$<close>")
                    break;

                Console.WriteLine($"[{DateTime.Now:T}] {message}, count cl -> {clients.Count}");
                Broadcast(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client error: {ex.Message}");
        }
        finally
        {
            lock (locker)
            {
                clients.RemoveAll(w => w.BaseStream == client.GetStream());
            }
            client.Close();
        }
    }

    private void Broadcast(string message)
    {
        lock (locker)
        {
            foreach (var client in clients.ToList())
            {
                try
                {
                    client.WriteLine(message);
                }
                catch
                {
                    clients.Remove(client);
                }
            }
        }
    }
}

internal class Program
{
    private static void Main(string[] args)
    {
        ChatServer chat = new ChatServer();
        chat.Start();
    }
}
