using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
/*
class Test
{
    private FileSaving _fileSaving;
    public Test()
    { 
        _fileSaving = new FileSaving("127.0.0.1", 50051);
    }

    static void Main(string[] args)
    {
        Test test = new Test();
        string filename = "cs_test_1";

        test.uploadFile(filename, Encoding.ASCII.GetBytes("test"), "");
        Console.WriteLine("upload finish");
        Console.ReadLine();

        test.downloadFile(filename);
        Console.WriteLine("download finish");
        Console.ReadLine();

        test.deleteFile(filename);
        Console.WriteLine("delete finish");

    }

    public async Task uploadFile(string filename, byte[] fileData, string type)
    {
        try
        {
            await this._fileSaving.uploadFile(filename, fileData, type);
        }
        catch(Exception ex)
        {
            throw new Exception("Error uploadFile");
        }
    }

    public async Task downloadFile(string filename)
    {
        try
        {
            byte[] fileBytes = await this._fileSaving.downloadFile(filename);
            Console.WriteLine(System.Text.Encoding.Default.GetString(fileBytes));
        }
        catch (Exception ex)
        {
            throw new Exception("Error downloadFile");
        }
    }
    public void deleteFile(string filename)
    {
        try
        {
            this._fileSaving.deleteFile(filename);
        }
        catch
        {
            Console.WriteLine("Error deleteFile");
        }
    }  
}
*/