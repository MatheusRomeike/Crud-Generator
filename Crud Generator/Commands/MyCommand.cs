﻿using Crud_Generator.Resources;
using Crud_Generator.Resources.Enum;
using EnvDTE;
using Microsoft;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.OLE.Interop;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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

            #region Dados locais
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
            Assumes.Present(dte);
            var activeDoc = dte.ActiveDocument;
            var selection = activeDoc.Selection as TextSelection;
            var codeClass = selection.ActivePoint.CodeElement[vsCMElement.vsCMElementClass] as CodeClass;

            string className = codeClass?.Name;

            if (className == null)
            {
                return;
            }
            #endregion

            #region Input
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
            }
            else
            {
                return;
            }
            #endregion

            #region Atributos
            Regex regexAtributos = new Regex(@"([A-Z_]+) +([A-Z_]+) *(not null)*");
            MatchCollection matchesAtributos = regexAtributos.Matches(input);

            List<AttributeInfo> listaAtributos = new List<AttributeInfo>
            {
                new AttributeInfo()
                {
                    Name = "MarcaId",
                    SqlName = "IDMARCA",
                    Type = "CHAVE_SMALLINT",
                    Nullable = false
                },
                new AttributeInfo()
                {
                    Name = "TipoSegmento",
                    SqlName = "TPSEGMENTO",
                    Type = "CHAVE_SMALLINT",
                    Nullable = false
                },
                new AttributeInfo()
                {
                    Name = "SequenciaConfiguracaoSnapOn",
                    SqlName = "SQCONFIGURACAOSNAPON",
                    Type = "CHAVE_SMALLINT",
                    Nullable = false
                },
                new AttributeInfo()
                {
                    Name = "TipoItem",
                    SqlName = "TPITEM",
                    Type = "CAMPO_SMALLINT",
                    Nullable = false
                },
                new AttributeInfo()
                {
                    Name = "CentroResultadoId",
                    SqlName = "CDCENTRORESULTADO",
                    Type = "CAMPO_SMALLINT",
                    Nullable = true
                }
            };

            foreach (Match match in matchesAtributos)
            {
                string nomeAtributo = Interaction.InputBox("Digite o nome do atributo " + match.Groups[1].Value + ":", "Mapear atributos", "");
                if (string.IsNullOrEmpty(nomeAtributo))
                    return;

                AttributeInfo atributo = new AttributeInfo()
                {
                    Name = nomeAtributo,
                    SqlName = match.Groups[1].Value,
                    Type = match.Groups[2].Value,
                    Nullable = match.Groups[3].Value != "not null"
                };
                listaAtributos.Add(atributo);
            };
            #endregion

            #region Primary key
            Regex regexPrimaryKey = new Regex(@"primary key \(([A-Z_]+(?:, [A-Z_]+)*)\)");
            Match matchPrimaryKey = regexPrimaryKey.Match(input);
            var primaryKeys = matchPrimaryKey.Groups[1].Value.Split(',');
            string primaryKeysString = "";
            foreach (var primaryKey in primaryKeys)
            {
                AttributeInfo atributo = listaAtributos.FirstOrDefault(a => a.SqlName == primaryKey.Trim());
                if (atributo != null)
                {
                    atributo.PrimaryKey = true;
                    if (primaryKeysString == "")
                    {
                        primaryKeysString += "x." + atributo.Name;
                    }
                    else
                    {
                        primaryKeysString += ", x." + atributo.Name;
                    }
                }
            }
            #endregion

            #region Foreign key
            Regex regexForeignKey = new Regex(@"foreign key\s*\(\s*(.+?)\s*\)\s*references\s*([A-Z_]+)\s*\(\s*(.+?)\s*\)");
            MatchCollection matchesForeignKey = regexForeignKey.Matches(input);

            foreach (Match match in matchesForeignKey)
            {
                ForeignType cardinalidade = (ForeignType)Convert.ToInt32(Interaction.InputBox("Digite a cardinalidade de " + match.Groups[1].Value + " na tabela " + match.Groups[2].Value + " com chave estrangeira: \n" +
                    "0 = Um pra um \n" +
                    "1 = Um pra muitos \n" +
                    "2 = Muitos pra um \n" +
                    "3 = Muitos pra muitos", "Mapear cardinalidade", ""));

                string nomeRelacionamento = Interaction.InputBox("Informe o nome da classe para relacionamento entre " + className + " e " + match.Groups[2].Value + ":", "Mapear relacionamento", "");
                if (string.IsNullOrEmpty(nomeRelacionamento))
                    return;

                ForeignKey foreignKey = new ForeignKey()
                {
                    ReferencesRelationName = nomeRelacionamento,
                    ReferencesSqlName = match.Groups[1].Value,
                    ForeignType = cardinalidade,
                };

                AttributeInfo atributo = listaAtributos.FirstOrDefault(a => a.SqlName.Contains(match.Groups[1].Value));
                if (atributo != null)
                    atributo.ForeignKey = foreignKey;
            }
            #endregion

            #region Table name
            Regex regexTableName = new Regex(@"create table ([A-Z]*)");
            Match matchTableName = regexTableName.Match(input);
            string tableName = matchTableName.Groups[1].Value;
            #endregion

            #endregion
#endregion

            #region Pegar caminhos locais
            string fullPath = activeDoc.FullName;
            string diretorioChamada = Path.GetDirectoryName(fullPath);
            string diretorioRaiz = Directory.GetCurrentDirectory();
            string classFolder = Path.GetFileName(diretorioChamada);

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

            #region Criar arquivos

            #region Criar Domain
            string domainContent =
            "using FluentValidation;\n" +
            "using Sisand.Vision.Domain." + classFolder + ".Validators;\n" +
            "namespace Sisand.Vision.Domain.SnapOn\n" +
            "{\n" +
            "\tpublic class " + className + " : Entity\n" +
            "\t{\n" +
            "\t\t#region Propriedades\n";

            foreach (var atributo in listaAtributos)
            {
                string tipoAtributo = "";
                if (atributo.Type == "CAMPO_BIGINT" || atributo.Type == "CAMPO_NOSSONUMERO")
                {
                    tipoAtributo = "long";
                }
                else if (atributo.Type == "CAMPO_INTEIRO" || atributo.Type == "CAMPO_SMALLINT" || atributo.Type == "CHAVE_INTEIRO" || atributo.Type == "CHAVE_SMALLINT")
                {
                    tipoAtributo = "int";
                }
                else if (atributo.Type == "CAMPO_BLOB")
                {
                    tipoAtributo = "byte[]";
                }
                else if (atributo.Type == "CAMPO_BOOLEAN")
                {
                    tipoAtributo = "bool";
                }
                else if (atributo.Type == "CAMPO_CPFCNPJ" || atributo.Type == "CAMPO_NOME" || atributo.Type == "CAMPO_NOME_PTBR" || atributo.Type == "CAMPO_OPERACAOCONTABIL" || atributo.Type == "CAMPO_RENAVAM" || atributo.Type == "CAMPO_UF" || atributo.Type == "CAMPO_VARCHAR1" || atributo.Type == "CAMPO_VARCHAR10" || atributo.Type == "CAMPO_VARCHAR100" || atributo.Type == "CAMPO_VARCHAR1000" || atributo.Type == "CAMPO_VARCHAR15" || atributo.Type == "CAMPO_VARCHAR150" || atributo.Type == "CAMPO_VARCHAR2" || atributo.Type == "CAMPO_VARCHAR20" || atributo.Type == "CAMPO_VARCHAR2000" || atributo.Type == "CAMPO_VARCHAR255" || atributo.Type == "CAMPO_VARCHAR3" || atributo.Type == "CAMPO_VARCHAR30" || atributo.Type == "CAMPO_VARCHAR3000" || atributo.Type == "CAMPO_VARCHAR355" || atributo.Type == "CAMPO_VARCHAR4" || atributo.Type == "CAMPO_VARCHAR50" || atributo.Type == "CAMPO_VARCHAR500" || atributo.Type == "CAMPO_VARCHAR5000" || atributo.Type == "CAMPO_VARCHAR6" || atributo.Type == "CAMPO_VARCHAR60")
                {
                    tipoAtributo = "string";
                }
                else if (atributo.Type == "CAMPO_DATA" || atributo.Type == "CAMPO_DATAHORA")
                {
                    tipoAtributo = "DateTime";
                }
                else if (atributo.Type == "CAMPO_DINHEIRO" || atributo.Type == "CAMPO_ITEMNOTA" || atributo.Type == "CAMPO_NUMERO" || atributo.Type == "CAMPO_NUMERO4D" || atributo.Type == "CAMPO_PERCENTUAL" || atributo.Type == "CAMPO_PERCENTUAL10D" || atributo.Type == "CAMPO_PESO")
                {
                    tipoAtributo = "decimal";
                }
                else if (atributo.Type == "CAMPO_HORA")
                {
                    tipoAtributo = "TimeSpan";
                }
                if (atributo.Nullable)
                    tipoAtributo += "?";

                domainContent += "\t\tpublic " + tipoAtributo + " " + atributo.Name + " { get; set; }\n";
            }

            domainContent += "\t\t#endregion\n" +
            "\n" +
            "\t\t#region Relacionamentos\nn { get; private set; }\n";

            foreach (var relacionamento in listaAtributos.Where(predicate: p => p.ForeignKey != null))
            {
                domainContent += "\t\tpublic virtual " + relacionamento.ForeignKey.ReferencesRelationName + " " + relacionamento.ForeignKey.ReferencesRelationName + " { get; set; }\n";
            }

            domainContent += "\t\t#endregion\n" +
            "\n" +
            "\t\t#region Constructor\n" +
            "\t\tpublic SnapOn() { }\n" +
            "\t\t#endregion\n" +
            "\n" +
            "\t\t#region Métodos\n" +
            "\t\tpublic override void IsValid() => new " + className + "Validator().ValidateAndThrow(this);\n" +
            "\t\t#endregion\n" +
            "\t}\n" +
            "}";

            File.WriteAllText(fullPath, domainContent);
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
                "\t\t\tbuilder.ToTable(" + tableName + ");\n" +
                "\n";
                if (primaryKeys.Count() > 1)
                {
                    configurationContent += "\t\t\tbuilder.HasKey(x => " + primaryKeysString + "); \n";
                }
                else
                {
                    configurationContent += "\t\t\tbuilder.HasKey(x => new { " + primaryKeysString + " }); \n";
                }

                foreach (var attribute in listaAtributos)
                {
                    configurationContent +=
                    "\n\t\t\tbuilder.Property(x => x." + attribute.Name + ")\n" +
                    "\t\t\t\t.HasColumnName(\"" + attribute.SqlName + "\")";
                    if (!attribute.Type.Contains("?") && !attribute.Type.Contains("string") && !attribute.Type.Contains("List") && !attribute.Type.Contains("IEnumerable"))
                    {
                        configurationContent += "\n\t\t\t\t.IsRequired()";
                    }
                    else if (attribute.Type == "bool")
                    {
                        configurationContent += "\n\t\t\t\t.HasConversion(new BoolToZeroOneConverter<int>())";
                    }
                    configurationContent += ";\n";
                }

                foreach (var attribute in listaAtributos.Where(predicate: p => p.ForeignKey != null))
                {
                    if (attribute.ForeignKey.ForeignType == Resources.Enum.ForeignType.UmPraUm)
                        configurationContent += "\t\t\t\tbuilder.HasOne(p => p." + attribute.ForeignKey.ReferencesRelationName + ") \n" +
                            "\t\t\t\t\t.WithOne(b => b." + className + ")\n";
                    else if (attribute.ForeignKey.ForeignType == Resources.Enum.ForeignType.UmPraMuitos)
                        configurationContent += "\t\t\t\tbuilder.HasOne(p => p." + attribute.ForeignKey.ReferencesRelationName + ") \n" +
                            "\t\t\t\t\t.WithMany(b => b." + className + ")\n";
                    else if (attribute.ForeignKey.ForeignType == Resources.Enum.ForeignType.MuitosPraUm)
                        configurationContent += "\t\t\t\tbuilder.HasMany(p => p." + attribute.ForeignKey.ReferencesRelationName + ") \n" +
                            "\t\t\t\t\t.WithOne(b => b." + className + ")\n";
                    else if (attribute.ForeignKey.ForeignType == Resources.Enum.ForeignType.UmPraMuitos)
                        configurationContent += "\t\t\t\tbuilder.HasMany(p => p." + attribute.ForeignKey.ReferencesRelationName + ") \n" +
                            "\t\t\t\t\t.WithMany(b => b." + className + ")\n";
                    var foreingKeys = attribute.ForeignKey.ReferencesSqlName.Split(',');
                    if (foreingKeys.Count() > 1)
                    {
                        configurationContent += "\t\t\t\t\t.HasForeignKey(p => new {";
                        foreach (var foreingKey in foreingKeys)
                        {
                            configurationFile += " p." + foreingKey.Trim() + ",";
                        }
                        configurationContent += "});\n";
                    }
                    else
                    {
                        configurationContent += "\t\t\t\t\t.HasForeignKey(p => p." + foreingKeys[0] + ");\n";
                    }

                }

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

            #endregion
        }

    }
}