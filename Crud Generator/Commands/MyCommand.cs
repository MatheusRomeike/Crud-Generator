﻿using Crud_Generator.Resources;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Crud_Generator
{
    [Command(PackageIds.MyCommand)]
    internal sealed class MyCommand : BaseCommand<MyCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            #region Pegar atributos
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
            Assumes.Present(dte);
            var activeDoc = dte.ActiveDocument;
            var selection = activeDoc.Selection as TextSelection;
            var codeClass = selection.ActivePoint.CodeElement[vsCMElement.vsCMElementClass] as CodeClass;
            #endregion

            #region Juntar atributos
            var listAttributes = new List<AttributeInfo>();

            foreach (CodeElement elem in codeClass.Children)
            {
                if (elem.Kind == vsCMElement.vsCMElementProperty)
                {
                    CodeProperty property = (CodeProperty)elem;
                    var atributo = new AttributeInfo()
                    {
                        Name = property.Name,
                        Type = property.Type.AsString
                    };
                    listAttributes.Add(atributo);
                };
            }
            #endregion

            #region Pegar dados locais
            string fullPath = activeDoc.FullName;
            string diretorioChamada = Path.GetDirectoryName(fullPath);
            string diretorioRaiz = Directory.GetCurrentDirectory();
            string classFolder = Path.GetFileName(diretorioChamada);
            string className = codeClass?.Name;

            if (className == null)
            {
                return;
            }

            string classNameFormatada = char.ToLower(className[0]) + className.Substring(1);

            //string domainFolder = Path.Combine(diretorioRaiz, "Domain");
            //string domainFolder2 = Path.Combine(domainFolder, "Domain");
            //string domainClassFolder = Path.Combine(domainFolder2, classFolder);
            //string dataFolder = Path.Combine(diretorioRaiz, "Data");
            //string dataRepositoriesFolder = Path.Combine(dataFolder, "Repository");
            //string applicationFolder = Path.Combine(diretorioRaiz, "Application");
            //string applicationContractsFolder = Path.Combine(applicationFolder, "Interfaces");
            //string controllerFolder = Path.Combine(diretorioRaiz, "Api");

            string domainFolder = Path.Combine(diretorioRaiz, "Sisand.Vision.Domain");
            string domainClassFolder = Path.Combine(domainFolder, classFolder);
            string dataFolder = Path.Combine(diretorioRaiz, "Sisand.Vision.Data");
            string dataConfigurationFolder = Path.Combine(dataFolder, "Configuration");
            string dataRepositoriesFolder = Path.Combine(dataFolder, "Repositories");
            string applicationFolder = Path.Combine(diretorioRaiz, "Sisand.Vision.Application");
            string applicationContractsFolder = Path.Combine(applicationFolder, "Contracts/Services");
            string applicationServicesFolder = Path.Combine(applicationFolder, "Services");
            string controllerFolder = Path.Combine(diretorioRaiz, "Sisand.Vision.Api");
            string controllerControllersFolder = Path.Combine(controllerFolder, "Controllers");
            #endregion

            #region Criar IRepository
            
            string repositoryInterfaceClassFolder = Path.Combine(domainClassFolder, "Contracts");
            Directory.CreateDirectory(repositoryInterfaceClassFolder);

            string repositoryInterfaceFile = Path.Combine(repositoryInterfaceClassFolder, "I" + className + "Repository.cs");
            if (!File.Exists(repositoryInterfaceFile))
            {
                File.Create(repositoryInterfaceFile).Dispose();

                string repositoryInterfaceContent = 
                    "using Sisand.Vision.Domain.Core.Contracts.Repositories;\n" +
                    "\n" +
                    "namespace Sisand.Vision.Domain." + className + ".Contracts\n" +
                    "{\n" +
                    "\tpublic interface I" + className + "Repository : IBaseRepository<" + className + ">\n" +
                    "\t{\n" +
                    "\t}\n" +
                    "}";
                File.WriteAllText(repositoryInterfaceFile, repositoryInterfaceContent);
            }
            #endregion

            #region Criar Repository
            string repositoryClassFolder = Path.Combine(dataRepositoriesFolder, classFolder);
            Directory.CreateDirectory(repositoryClassFolder);

            string repositoryFile = Path.Combine(repositoryClassFolder, className + "Repository.cs");
            if (!File.Exists(repositoryFile))
            {
                File.Create(repositoryFile).Dispose();

                string repositoryContent =
                    "using Domain.Domain.Core.Contracts;\n" +
                    "using Microsoft.EntityFrameworkCore;\n" +
                    "using Sisand.Vision.Data.DataContext;\n" +
                    "using Sisand.Vision.Domain.NegociacaoVenda.Contracts;" +
                    "\n" +
                    "namespace Sisand.Vision.Data.Repositories." + classFolder + "\n" +
                    "{\n" +
                    "\tpublic class " + className + "Repository : BaseRepository<Domain." + classFolder + "." + className + ">, I" + className + "Repository\n" +
                    "\t{\n" +
                    "\t\t#region Atributos\n" +
                    "\t\tprivate readonly DapperContext _dapperContext;\n" +
                    "\t\t#endregion\n" +
                    "\n" +
                    "\t\t#region Construtor\n" +
                    "\t\tpublic " + className + "Repository(EFContext context, DapperContext dapperContext) : base(context)\n" +
                    "\t\t{\n" +
                    "\t\t\t_dapperContext = dapperContext;\n" +
                    "\t\t}\n" +
                    "\t\t#endregion\n" +
                    "\n" +
                    "\t\t#region Queries\n" +
                    "\n" +
                    "\t\t#endregion\n" +
                    "\n" +
                    "\t\t#region Metodos\n" +
                    "\n" +
                    "\t\t#endregion\n" +
                    "\t}" +
                    "}\n";
                File.WriteAllText(repositoryFile, repositoryContent);
            }
            #endregion

            #region Criar Configuration
            string configurationFile = Path.Combine(dataConfigurationFolder, className + "Configuration.cs");
            if (!File.Exists(configurationFile))
            {
                File.Create(configurationFile).Dispose();

                string configurationContent =
                    "using Microsoft.EntityFrameworkCore;\n" +
                    "using Microsoft.EntityFrameworkCore.Metadata.Builders;\n" +
                    "using Microsoft.EntityFrameworkCore.Storage.ValueConversion;\n" +
                    "using Sisand.Vision.Domain." + classFolder + ";\n" + //falta mexer nisso
                    "\n" +
                    "namespace Sisand.Vision.Data.Configurations\n" +
                    "{\n" +
                    "\tpublic class " + className + "Configuration : IEntityTypeConfiguration<" + className + ">\n" +
                    "\t{\n" +
                    "\t\tpublic void Configure(EntityTypeBuilder<" + className + "> builder)\n" +
                    "\t\t{\n" +
                    "\t\t\tbuilder.ToTable(\"PREENCHA\");\n" +
                    "\n" +
                    "\t\t\tbuilder.HasKey(\"PREENCHA\"); \n";
                foreach (var attribute in listAttributes)
                {
                    configurationContent +=
                    "\n\t\t\tbuilder.Property(x => x." + attribute.Name + ")\n" +
                    "\t\t\t\t.HasColumnName(\"PREENCHA\")";
                    if (attribute.Type.Contains("?"))
                    {
                        configurationContent += "\n\t\t\t\t.IsRequired()";
                    }
                    else if (attribute.Type == "bool")
                    {
                        configurationContent += "\n\t\t\t\t.HasConversion(new BoolToZeroOneConverter<int>())";
                    }
                    configurationContent += ";\n";
                };
                configurationContent +=
                "\t\t}\n" +
                "\t}\n" +
                "}";

                File.WriteAllText(configurationFile, configurationContent);
            }
            #endregion

            #region Criar IService
            string applicationContractsClassFolder = Path.Combine(applicationContractsFolder, classFolder);
            Directory.CreateDirectory(applicationContractsClassFolder);

            string serviceInterfaceFile = Path.Combine(applicationContractsClassFolder, "I" + className + "Service.cs");
            if (!File.Exists(serviceInterfaceFile))
            {
                File.Create(serviceInterfaceFile).Dispose();

                string serviceInterfaceContent =
                    "using Sisand.Vision.Domain." + classFolder + ";\n" +
                    "namespace Sisand.Vision.Application.Contracts.Services." + classFolder + "\n" +
                    "{\n" +
                    "\tpublic interface I" + className + "Service\n" +
                    "\t{\n" +
                    "\t\t/// <summary>\n" +
                    "\t\t/// Método responsável por adicionar uma entidade do tipo " + className + "\n" +
                    "\t\t/// </summary>\n" +
                    "\t\t/// <param name=\"entity\"></param>\n" +
                    "\t\tvoid Add(" + className + " entity);\n" +
                    "\n" +
                    "\t\t/// <summary>\n" +
                    "\t\t/// Método responsável por atualizar uma entidade do tipo " + className + "\n" +
                    "\t\t/// </summary>\n" +
                    "\t\t/// <param name=\"entity\"></param>\n" +
                    "\t\tvoid Update(" + className + " entity);\n" +
                    "\n" +
                    "\t\t/// <summary>\n" +
                    "\t\t/// Método responsável por remover uma entidade do tipo " + className + "\n" +
                    "\t\t/// </summary>\n" +
                    "\t\t/// <param name=\"entity\"></param>\n" +
                    "\t\tvoid Delete(" + className + " entity);\n" +
                    "\n" +
                    "\t}\n" +
                    "}";
                File.WriteAllText(serviceInterfaceFile, serviceInterfaceContent);
            }
            #endregion

            #region Criar Service
            string applicationServicesClassFolder = Path.Combine(applicationServicesFolder, classFolder);
            Directory.CreateDirectory(applicationServicesClassFolder);

            string serviceFile = Path.Combine(applicationServicesClassFolder, className + "Service.cs");
            if (!File.Exists(serviceFile))
            {
                File.Create(serviceFile).Dispose();

                string serviceContent =
                    "using Sisand.Vision.Application.Contracts.Services." + classFolder + ";\n" +
                    "using Sisand.Vision.Data.Contracts;\n" +
                    "using Sisand.Vision.Domain." + classFolder + ".Contracts;\n" +
                    "using Sisand.Vision.Domain." + classFolder + ";\n" +
                    "namespace Sisand.Vision.Application.Services." + classFolder + "\n" +
                    "{\n" +
                    "\tpublic class " + className + "Service : I" + className + "Service\n" +
                    "\t{\n" +
                    "\t\t#region Atributos\n" +
                    "\t\tprivate readonly I" + className + "Repository _" + classNameFormatada + "Repository;\n" +
                    "\t\tprivate readonly IUnitOfWork _unitOfWork;\n" +
                    "\t\t#endregion\n" +
                    "\n" +
                    "\t\t#region Construtor\n" +
                    "\t\tpublic " + className + "Service(I" + className + "Repository " + classNameFormatada + "Service, IUnitOfWork unitOfWork)\n" +
                    "\t\t{\n" +
                    "\t\t\t_" + classNameFormatada + "Repository = " + classNameFormatada + "Service;\n" +
                    "\t\t\t_unitOfWork = unitOfWork;\n" +
                    "\t\t}\n" +
                    "\t\t#endregion\n" +
                    "\n" +
                    "\t\t#region Metodos\n" +
                    "\t\tpublic void Add(" + className + " entity)\n" +
                    "\t\t{\n" +
                    "\t\t\t_" + classNameFormatada + "Repository.Add(entity);\n" +
                    "\t\t}\n" +
                    "\n" +
                    "\t\tpublic void Update(" + className + " entity)\n" +
                    "\t\t{\n" +
                    "\t\t\t_" + classNameFormatada + "Repository.Update(entity);\n" +
                    "\t\t}\n" +
                    "\n" +
                    "\t\tpublic void Delete(" + className + " entity)\n" +
                    "\t\t{\n" +
                    "\t\t\t_" + classNameFormatada + "Repository.Delete(entity);\n" +
                    "\t\t}\n" +
                    "\t\t#endregion\n" +
                    "\t}\n" +
                    "}";
                File.WriteAllText(serviceFile, serviceContent);
            }
            #endregion

            #region Criar Controller
            string controllerFile = Path.Combine(controllerControllersFolder, className + "Controller.cs");
            if (!File.Exists(controllerFile))
            {
                File.Create(controllerFile).Dispose();

                string controllerContent =
                    "using Microsoft.AspNetCore.Authorization;\n" +
                    "using Microsoft.AspNetCore.Mvc;\n" +
                    "using Sisand.Vision.Application.Contracts.Services." + className + ";\n" +
                    "using Sisand.Vision.Domain." + classFolder + ";\n" +
                    "\nnamespace Sisand.Vision.Api.Controllers\n" +
                    "{\n" +
                    "\t[Route(\"api/[controller]\")]\n" +
                    "\t[Produces(\"application/json\")]\n" +
                    "\t[ApiController]\n" +
                    "\t[Authorize]\n" +
                    "\tpublic class " + className + "Controller : BaseController\n" +
                    "\t{\n" +
                    "\t\t#region Atributos\n" +
                    "\t\tprivate readonly I" + className + "Service _" + classNameFormatada + "Service;\n" +
                    "\t\t#endregion\n" +
                    "\n" +
                    "\t\t#region Constructor\n" +
                    "\t\tpublic " + className + "Controller(I" + className + "Service " + classNameFormatada + "Service)\n" +
                    "\t\t{\n" +
                    "\t\t\t_" + classNameFormatada + "Service = " + classNameFormatada + "Service;\n" +
                    "\t\t}\n" +
                    "\t\t#endregion\n" +
                    "\n" +
                    "\t\t#region HttpPost\n" +
                    "\t\t/// <summary>\n" +
                    "\t\t/// Método responsável por adicionar uma entidade do tipo " + className + "\n" +
                    "\t\t/// </summary>\n" +
                    "\t\t/// <param name=\"entity\"></param>\n" +
                    "\t\t/// <returns></returns>\n" +
                    "\t\t[HttpPost(\"Add\")]\n" +
                    "\t\t[ProducesResponseType(typeof(RetornoPadrao<dynamic>), 200)]\n" +
                    "\t\t[ProducesResponseType(typeof(Exception), 400)]\n" +
                    "\t\t[ProducesResponseType(500)]\n" +
                    "\t\tpublic Task<IActionResult> Add([FromBody] " + className + " entity)\n" +
                    "\t\t{\n" +
                    "\t\t\treturn Task.Run(() =>\n" +
                    "\t\t\t{\n" +
                    "\t\t\t\ttry\n" +
                    "\t\t\t\t{\n" +
                    "\t\t\t\t\treturn Ok(new RetornoPadrao<dynamic>(EStatusRetorno.Ok, _vendaPerdidaService.Add(entity)));\n" +
                    "\t\t\t\t}\n" +
                    "\t\t\t\tcatch (Exception e)\n" +
                    "\t\t\t\t{\n" +
                    "\t\t\t\t\treturn ResolveErro(e);\n" +
                    "\t\t\t\t}\n" +
                    "\t\t\t});\n" +
                    "\t\t}\n" +
                    "\t\t#endregion\n" +
                    "\n" +
                    "\t\t#region HttpPut\n" +
                    "\t\t/// <summary>\n" +
                    "\t\t/// Método responsável por atualizar uma entidade do tipo " + className + "\n" +
                    "\t\t/// </summary>\n" +
                    "\t\t/// <param name=\"entity\"></param>\n" +
                    "\t\t/// <returns></returns>\n" +
                    "\t\t[HttpPost(\"Update\")]\n" +
                    "\t\t[ProducesResponseType(typeof(RetornoPadrao<dynamic>), 200)]\n" +
                    "\t\t[ProducesResponseType(typeof(Exception), 400)]\n" +
                    "\t\t[ProducesResponseType(500)]\n" +
                    "\t\tpublic Task<IActionResult> Update([FromBody] " + className + " entity)\n" +
                    "\t\t{\n" +
                    "\t\t\treturn Task.Run(() =>\n" +
                    "\t\t\t{\n" +
                    "\t\t\t\ttry\n" +
                    "\t\t\t\t{\n" +
                    "\t\t\t\t\treturn Ok(new RetornoPadrao<dynamic>(EStatusRetorno.Ok, _vendaPerdidaService.Update(entity)));\n" +
                    "\t\t\t\t}\n" +
                    "\t\t\t\tcatch (Exception e)\n" +
                    "\t\t\t\t{\n" +
                    "\t\t\t\t\treturn ResolveErro(e);\n" +
                    "\t\t\t\t}\n" +
                    "\t\t\t});\n" +
                    "\t\t}\n" +
                    "\t\t#endregion\n" +
                    "\n" +
                    "\t\t#region HttpPost\n" +
                    "\t\t/// <summary>\n" +
                    "\t\t/// Método responsável por remover uma entidade do tipo " + className + "\n" +
                    "\t\t/// </summary>\n" +
                    "\t\t/// <param name=\"entity\"></param>\n" +
                    "\t\t/// <returns></returns>\n" +
                    "\t\t[HttpPost(\"Delete\")]\n" +
                    "\t\t[ProducesResponseType(typeof(RetornoPadrao<dynamic>), 200)]\n" +
                    "\t\t[ProducesResponseType(typeof(Exception), 400)]\n" +
                    "\t\t[ProducesResponseType(500)]\n" +
                    "\t\tpublic Task<IActionResult> Delete([FromBody] " + className + " entity)\n" +
                    "\t\t{\n" +
                    "\t\t\treturn Task.Run(() =>\n" +
                    "\t\t\t{\n" +
                    "\t\t\t\ttry\n" +
                    "\t\t\t\t{\n" +
                    "\t\t\t\t\treturn Ok(new RetornoPadrao<dynamic>(EStatusRetorno.Ok, _vendaPerdidaService.Delete(entity)));\n" +
                    "\t\t\t\t}\n" +
                    "\t\t\t\tcatch (Exception e)\n" +
                    "\t\t\t\t{\n" +
                    "\t\t\t\t\treturn ResolveErro(e);\n" +
                    "\t\t\t\t}\n" +
                    "\t\t\t});\n" +
                    "\t\t}\n" +
                    "\t\t#endregion\n" +
                    "\t}\n" +
                    "}";
                File.WriteAllText(controllerFile, controllerContent);
            }
            #endregion
        }

    }
}