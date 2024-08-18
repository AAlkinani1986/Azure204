
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System.IO;

Console.WriteLine("Azure Blob Storage - .NET Core Quickstart\n");

// Run the examples asynchronously, wait for the results before proceeding
ProcessAsync().GetAwaiter().GetResult();

Console.WriteLine("Press enter to exit");
Console.ReadLine();

static async Task ProcessAsync(){
    // Load the configuration from the appsettings.json file
 // Load the configuration from the appsettings.json file
      // Build the configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

   
      
    string storageConnectionString = configuration.GetSection("AzureStorage:ConnectionString")?.Value ?? string.Empty;

    BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);

    // Create a unique name for the container
    string containerName = "wtblob" + Guid.NewGuid().ToString();

    BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
    Console.WriteLine($"A Container name '{containerClient.Name}'" + " has been created" +
    "\nTake a minute and verify in the Azure Portal" + 
    "\nNext the file will be created and uploaded to the container");
    Console.WriteLine("Press enter to continue");
    Console.ReadLine();
    // create a local file in the ./data/ directory for uploading and downloading
    string localPath = "./data/";
    string fileName = "wtfile" + Guid.NewGuid().ToString() + ".txt";
    string localFilePath = Path.Combine(localPath, fileName);

    // Write text to the file
    await File.WriteAllTextAsync(localFilePath, "Hello, World!");

    // Get a reference to a blob
    BlobClient blobClient = containerClient.GetBlobClient(fileName);
    Console.WriteLine($"Uploading to Blob storage as blob:\n\t {blobClient.Uri}");
    // Open the file and upload its data
    using (FileStream uploadFileStream = File.OpenRead(localFilePath))
    {
        await blobClient.UploadAsync(uploadFileStream, true);
        uploadFileStream.Close();
    }

    Console.WriteLine("\nThe file was uploaded. we'll verify by listing the blobs in the container");
    Console.WriteLine("Press enter to continue");
    Console.ReadLine();

    // list the blobs in the container

    Console.WriteLine("Listing blobs...");
    await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
    {
        Console.WriteLine("\t" + blobItem.Name);
    }
    Console.WriteLine("\nYou can also verify by looking inside the container in the Azure Portal" + 
    "\nNext the blob will be downloaded with an altered file name.");
    Console.WriteLine("Press enter to continue");
    Console.ReadLine();

    // Download the blob to a local file
    // Append the string "DOWNLOADED" before the .txt extension so you can compare the files in the data directory
    string downloadFilePath = localFilePath.Replace(".txt", "DOWNLOADED.txt");
    Console.WriteLine($"Downloading blob to\n\t{downloadFilePath}\n");
    // Download the blob's contents and save it to a file
    BlobDownloadInfo download = await blobClient.DownloadAsync();
    
    using (FileStream downloadFileStream = File.OpenWrite(downloadFilePath))
    {
        await download.Content.CopyToAsync(downloadFileStream);
        downloadFileStream.Close();
    }

    Console.WriteLine("\nThe blob was downloaded and saved to the file. You can verify by comparing the contents of the file in the data directory");
    Console.WriteLine("Press enter to continue");
    Console.ReadLine();

    // Delete the container and clean up local files created
    Console.WriteLine("\n\nDelete blob container ...");
    await containerClient.DeleteAsync();

    Console.WriteLine("Deleting the local source and downloaded files");
    File.Delete(localFilePath);
    File.Delete(downloadFilePath);
    
    Console.WriteLine("Finished cleanup");

}