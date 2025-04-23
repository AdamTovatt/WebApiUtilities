using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System;

namespace WebApiUtilities.Resources
{
    /// <summary>
    /// Helper class for working with embedded resources in the application.
    /// </summary>
    public class ResourceHelper
    {
        private static ResourceHelper? instance;

        /// <summary>
        /// Singleton instance of the <see cref="ResourceHelper"/>. Call the ResourceHelper.Initialize() function before using this.
        /// </summary>
        public static ResourceHelper Instance
        {
            get
            {
                if (instance == null) throw new InvalidOperationException($"An attempt to get a uninitialized {nameof(ResourceHelper)} was made! Call {nameof(ResourceHelper)}.{nameof(Initialize)}() before trying to get an instance.");
                return instance;
            }
        }

        private readonly EmbeddedFileProvider fileProvider;

        private ResourceHelper(Assembly assembly)
        {
            fileProvider = new EmbeddedFileProvider(assembly);
        }

        /// <summary>
        /// Gets the MIME content type based on the resource file extension.
        /// </summary>
        /// <param name="resource">The resource to evaluate.</param>
        /// <returns>The corresponding content type string.</returns>
        public string GetContentType(Resource resource)
        {
            string extension = Path.GetExtension(resource.Path).ToLowerInvariant();

            return extension switch
            {
                ".html" => "text/html",
                ".otf" => "font/otf",
                ".ttf" => "font/ttf",
                ".svg" => "image/svg+xml",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".pdf" => "application/pdf",
                ".cer" => "application/x-x509-ca-cert",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// Reads the content of the resource as a string asynchronously.
        /// </summary>
        /// <param name="resource">The resource to read.</param>
        /// <returns>The resource content as a string.</returns>
        public async Task<string> ReadAsStringAsync(Resource resource)
        {
            using (Stream resourceStream = GetFileStream(resource))
            {
                using (StreamReader reader = new StreamReader(resourceStream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        /// <summary>
        /// Gets a stream for reading the resource content.
        /// </summary>
        /// <param name="resource">The resource to read.</param>
        /// <returns>A readable stream of the resource content.</returns>
        public Stream GetFileStream(Resource resource)
        {
            return GetFileInfo(resource).CreateReadStream();
        }

        /// <summary>
        /// Gets file info for a given resource.
        /// </summary>
        /// <param name="resource">The resource to look up.</param>
        /// <returns>The file info of the resource.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the file is not found.</exception>
        public IFileInfo GetFileInfo(Resource resource)
        {
            string fullPath = $"Resources/{resource.Path}";
            IFileInfo fileInfo = fileProvider.GetFileInfo(fullPath);

            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException($"Resource '{fullPath}' not found.");
            }

            return fileInfo;
        }

        /// <summary>
        /// Initializes the <see cref="ResourceHelper"/> instance while and also verifying that all embedded resources have corresponding mappings and vice versa.
        /// </summary>
        /// <param name="assembly">Optional parameter to explicitly specify which assembly the embedded resources exist in.</param>
        /// <exception cref="InvalidOperationException">Thrown if there are mismatches.</exception>
        public static void Initialize(Assembly? assembly = null)
        {
            Assembly assemblyToUse = assembly ?? Assembly.GetCallingAssembly();
            instance = new ResourceHelper(assemblyToUse);

            StringBuilder errors = new StringBuilder();

            HashSet<string> mappedResources = GetAllResourcePaths();
            foreach (string resourcePath in mappedResources)
            {
                string fullPath = $"Resources/{resourcePath}";
                IFileInfo fileInfo = instance.fileProvider.GetFileInfo(fullPath);
                if (!fileInfo.Exists)
                {
                    errors.AppendLine($"❌ Missing Embedded Resource: {fullPath}");
                }
            }

            HashSet<string> embeddedResources = instance.GetEmbeddedResourcePaths();
            foreach (string embeddedFile in embeddedResources)
            {
                if (!mappedResources.Contains(embeddedFile))
                {
                    errors.AppendLine($"❌ Unmapped Embedded File: {embeddedFile}");
                }
            }

            if (errors.Length > 0)
            {
                StringBuilder errorMessageBuilder = new StringBuilder($"Resource mapping integrity check failed:\n{errors}");
                errorMessageBuilder.AppendLine("If an embedded resource is missing it means that an embedded resources from the Resources folder has been removed while the mapping for it in the Resource class remains.");
                errorMessageBuilder.AppendLine("If there is an unmapped embedded file it means that there is an embedded resource in the Resources folder that has not been mapped correctly in the Resource class.");
                errorMessageBuilder.AppendLine("This error message and check is to make sure that all expected embedded resources in the Resources folder exist.");
                errorMessageBuilder.AppendLine("It is performed when the tests run and on api startup to ensure no unexpected error can occur in the middle of resonding to a request or similar.");

                throw new InvalidOperationException(errorMessageBuilder.ToString());
            }
        }

        /// <summary>
        /// Retrieves all resource paths defined in the <see cref="Resource"/> mappings.
        /// </summary>
        /// <returns>A set of all mapped resource paths.</returns>
        private static HashSet<string> GetAllResourcePaths()
        {
            HashSet<string> paths = new HashSet<string>();

            Type[] categories = typeof(Resource).GetNestedTypes();
            foreach (Type category in categories)
            {
                FieldInfo[] fields = category.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (FieldInfo field in fields)
                {
                    object? fieldValue = field.GetValue(null);
                    if (fieldValue is Resource resource)
                    {
                        paths.Add(resource.Path);
                    }
                }
            }

            return paths;
        }

        /// <summary>
        /// Gets the names of all folders that contain resource mappings.
        /// </summary>
        /// <returns>A set of folder names.</returns>
        private HashSet<string> GetAllResourceFolders()
        {
            HashSet<string> folders = new HashSet<string>();

            foreach (Type category in typeof(Resource).GetNestedTypes())
            {
                if (category.GetFields(BindingFlags.Public | BindingFlags.Static).Any(x => x.GetValue(null) is Resource))
                    folders.Add(category.Name);
            }

            return folders;
        }

        /// <summary>
        /// Gets the paths of all embedded resources within the assembly.
        /// </summary>
        /// <returns>A set of embedded resource paths.</returns>
        private HashSet<string> GetEmbeddedResourcePaths()
        {
            HashSet<string> embeddedResources = new HashSet<string>();

            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyName = assembly.GetName().Name!;
            string[] resourceNames = assembly.GetManifestResourceNames();

            HashSet<string> knownFolders = GetAllResourceFolders();

            foreach (string resource in resourceNames)
            {
                if (resource.StartsWith($"{assemblyName}.Resources"))
                {
                    string relativePath = resource.Substring($"{assemblyName}.Resources.".Length);
                    string[] parts = relativePath.Split('.');
                    List<string> processedParts = new List<string>();

                    for (int i = 0; i < parts.Length; i++)
                    {
                        string currentPart = parts[i];
                        string constructedCheckPath = string.Join("/", processedParts) + "/" + currentPart;
                        if (processedParts.Count == 0 && constructedCheckPath.Length > 1) constructedCheckPath = constructedCheckPath.Substring(1);

                        if (knownFolders.Contains(constructedCheckPath))
                        {
                            processedParts.Add(currentPart);
                        }
                        else
                        {
                            string filename = string.Join(".", parts.Skip(i));
                            processedParts.Add(filename);
                            break;
                        }
                    }

                    string normalizedPath = string.Join("/", processedParts);
                    embeddedResources.Add(normalizedPath);
                }
            }

            return embeddedResources;
        }
    }
}
