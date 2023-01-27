using Crud_Generator.Resources;
using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio.OLE.Interop;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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

            RichTextBox richTextBox = new RichTextBox();
            richTextBox.Dock = DockStyle.Fill;
            Form form = new Form();
            form.Text = "Insira o script do modelo.";
            form.Size = new Size(500, 500);


            Panel panel = new Panel();
            panel.Dock = DockStyle.Bottom;
            panel.Height = 25;
            form.Controls.Add(panel);

            Button btnConfirmar = new Button();
            btnConfirmar.Text = "Confirmar";
            btnConfirmar.DialogResult = DialogResult.OK;
            btnConfirmar.Width = 75;
            btnConfirmar.Left = form.Width - btnConfirmar.Width - 15;
            panel.Controls.Add(btnConfirmar);

            Button btnCancelar = new Button();
            btnCancelar.Text = "Cancelar";
            btnCancelar.Width = 75;
            btnCancelar.Left = btnConfirmar.Left - 30 - btnCancelar.Width;
            btnCancelar.DialogResult = DialogResult.Cancel;
            panel.Controls.Add(btnCancelar);


            form.Controls.Add(richTextBox);

            string input = "";

            if (form.ShowDialog() == DialogResult.OK)
            {
                input = richTextBox.Text;
            } else
            {
                return;
            }

            Regex regexAtributos = new Regex(@"([A-Z_]+) +([A-Z_]+) *(not null)*");
            MatchCollection matches = regexAtributos.Matches(input);

            List<AttributeInfo> listaAtributos = new List<AttributeInfo>();

            foreach (Match match in matches)
            {
                AttributeInfo atributo = new AttributeInfo()
                {
                    Name = match.Groups[1].Value,
                    Type = match.Groups[2].Value,
                    Nullable = match.Groups[3].Value != "not null"
                };
                listaAtributos.Add(atributo);
            };

            return;


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

            string domainFolder = Path.Combine(diretorioRaiz, "Sisand.Vision.Domain");
            string domainClassFolder = Path.Combine(domainFolder, classFolder);

            string dataFolder = Path.Combine(diretorioRaiz, "Sisand.Vision.Data");
            string dataConfigurationFolder = Path.Combine(dataFolder, "Configurations");
            string dataRepositoriesFolder = Path.Combine(dataFolder, "Repositories");

            string applicationFolder = Path.Combine(diretorioRaiz, "Sisand.Vision.Application");
            string applicationContractsFolder = Path.Combine(applicationFolder, "Contracts");
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
                    "namespace Sisand.Vision.Domain." + classFolder + ".Contracts\n" +
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
                    "using Microsoft.EntityFrameworkCore;\n" +
                    "using Sisand.Vision.Data.DataContext;\n" +
                    "using Sisand.Vision.Domain." + classFolder + ".Contracts;" +
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
                    "\t}\n" +
                    "}\n";
                File.WriteAllText(repositoryFile, repositoryContent);
            }
            #endregion

            #region Criar Validator
            string validatorsClassFolder = Path.Combine(domainClassFolder, "Validators");
            Directory.CreateDirectory(validatorsClassFolder);

            string validatorFile = Path.Combine(validatorsClassFolder, className + "Validator.cs");
            if (!File.Exists(validatorFile))
            {
                File.Create(validatorFile).Dispose();

                string validatorContent =
                    "using FluentValidation;\n" +
                    "namespace Sisand.Vision.Domain." + classFolder + ".Validators\n" +
                    "{\n" +
                    "\tpublic class " + className + "Validator : AbstractValidator<" + className + ">\n" +
                    "\t{\n" +
                    "\t\tpublic " + className + "Validator()\n" +
                    "\t\t{\n" +
                    "\t\t}\n" +
                    "\t}\n" +
                    "}";
                File.WriteAllText(validatorFile, validatorContent);
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
                /*
                foreach (var attribute in listAttributes)
                {
                    configurationContent +=
                    "\n\t\t\tbuilder.Property(x => x." + attribute.Name + ")\n" +
                    "\t\t\t\t.HasColumnName(\"PREENCHA\")";
                    if (!attribute.Type.Contains("?") && !attribute.Type.Contains("string") && !attribute.Type.Contains("List") && !attribute.Type.Contains("IEnumerable"))
                    {
                        configurationContent += "\n\t\t\t\t.IsRequired()";
                    }
                    else if (attribute.Type == "bool")
                    {
                        configurationContent += "\n\t\t\t\t.HasConversion(new BoolToZeroOneConverter<int>())";
                    }
                    configurationContent += ";\n";
                };
                */
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
                    "\t\tbool Add(" + className + " entity);\n" +
                    "\n" +
                    "\t\t/// <summary>\n" +
                    "\t\t/// Método responsável por atualizar uma entidade do tipo " + className + "\n" +
                    "\t\t/// </summary>\n" +
                    "\t\t/// <param name=\"entity\"></param>\n" +
                    "\t\tbool Update(" + className + " entity);\n" +
                    "\n" +
                    "\t\t/// <summary>\n" +
                    "\t\t/// Método responsável por remover uma entidade do tipo " + className + "\n" +
                    "\t\t/// </summary>\n" +
                    "\t\t/// <param name=\"entity\"></param>\n" +
                    "\t\tbool Delete(" + className + " entity);\n" +
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
                    "\t\tpublic bool Add(" + className + " entity)\n" +
                    "\t\t{\n" +
                    "\t\t\t_" + classNameFormatada + "Repository.Add(entity);\n" +
                    "\t\t\t_unitOfWork.EFCommit();\n" +
                    "\t\t\treturn true;\n" +
                    "\t\t}\n" +
                    "\n" +
                    "\t\tpublic bool Update(" + className + " entity)\n" +
                    "\t\t{\n" +
                    "\t\t\t_" + classNameFormatada + "Repository.Update(entity);\n" +
                    "\t\t\t_unitOfWork.EFCommit();\n" +
                    "\t\t\treturn true;\n" +
                    "\t\t}\n" +
                    "\n" +
                    "\t\tpublic bool Delete(" + className + " entity)\n" +
                    "\t\t{\n" +
                    "\t\t\t_" + classNameFormatada + "Repository.Delete(entity);\n" +
                    "\t\t\t_unitOfWork.EFCommit();\n" +
                    "\t\t\treturn true;\n" +
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
                    "using Sisand.Vision.Api.Utils;\n" +
                    "using Sisand.Vision.Application.Contracts.Services." + classFolder + ";\n" +
                    "using Sisand.Vision.Domain." + classFolder + ";\n" +
                    "using System;\n" +
                    "using System.Threading.Tasks;\n" +
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
                    "\t\t[ProducesResponseType(typeof(RetornoPadrao<bool>), 200)]\n" +
                    "\t\t[ProducesResponseType(typeof(Exception), 400)]\n" +
                    "\t\t[ProducesResponseType(500)]\n" +
                    "\t\tpublic Task<IActionResult> Add([FromBody] " + className + " entity)\n" +
                    "\t\t{\n" +
                    "\t\t\treturn Task.Run(() =>\n" +
                    "\t\t\t{\n" +
                    "\t\t\t\ttry\n" +
                    "\t\t\t\t{\n" +
                    "\t\t\t\t\treturn Ok(new RetornoPadrao<bool>(EStatusRetorno.Ok, _" + classNameFormatada + "Service.Add(entity)));\n" +
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
                    "\t\t[ProducesResponseType(typeof(RetornoPadrao<bool>), 200)]\n" +
                    "\t\t[ProducesResponseType(typeof(Exception), 400)]\n" +
                    "\t\t[ProducesResponseType(500)]\n" +
                    "\t\tpublic Task<IActionResult> Update([FromBody] " + className + " entity)\n" +
                    "\t\t{\n" +
                    "\t\t\treturn Task.Run(() =>\n" +
                    "\t\t\t{\n" +
                    "\t\t\t\ttry\n" +
                    "\t\t\t\t{\n" +
                    "\t\t\t\t\treturn Ok(new RetornoPadrao<bool>(EStatusRetorno.Ok, _" + classNameFormatada + "Service.Update(entity)));\n" +
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
                    "\t\t[ProducesResponseType(typeof(RetornoPadrao<bool>), 200)]\n" +
                    "\t\t[ProducesResponseType(typeof(Exception), 400)]\n" +
                    "\t\t[ProducesResponseType(500)]\n" +
                    "\t\tpublic Task<IActionResult> Delete([FromBody] " + className + " entity)\n" +
                    "\t\t{\n" +
                    "\t\t\treturn Task.Run(() =>\n" +
                    "\t\t\t{\n" +
                    "\t\t\t\ttry\n" +
                    "\t\t\t\t{\n" +
                    "\t\t\t\t\treturn Ok(new RetornoPadrao<bool>(EStatusRetorno.Ok, _" + classNameFormatada + "Service.Delete(entity)));\n" +
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