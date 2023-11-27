using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

class Test
{
    private FileSaving _fileSaving;
    public Test()
    { 
        _fileSaving = new FileSaving();
    }

    static void Main(string[] args)
    {
        Test test = new Test();
        test.uploadFile("cs test.txt", Encoding.ASCII.GetBytes("test"), "");
        Console.WriteLine("upload finsh");
        test.downloadFile("cs test.txt");
        Console.WriteLine("download finsh");
        test.deleteFile("cs test.txt");
        Console.WriteLine("delete finish");

    }

    public async Task uploadFile(string filename, byte[] fileData, string type)
    {
        try
        {
            this._fileSaving.uploadFile(filename, fileData, type);
        }
        catch
        {
            Console.WriteLine("Error uploadFile");
        }
    }

    public async Task  downloadFile(string filename)
    {
        try
        {
            byte[] fileBytes = await this._fileSaving.downloadFile(filename);
            Console.WriteLine(System.Text.Encoding.Default.GetString(fileBytes));
        }
        catch 
        {
            Console.WriteLine("Error downloadFile");
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