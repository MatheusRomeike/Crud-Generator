using Crud_Generator.Resources;
using Crud_Generator.Resources.Enum;
using Crud_Generator.Resources.Forms;
using EnvDTE;
using Microsoft;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text.Differencing;
using System;
using System.Collections.Generic;
using System.Data;
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

            string sql = ObterSQL();
            List<AttributeInfo> listaAtributos = DeParaAtributos(sql);
            var primaryKey = GerarStringPrimaryKey(sql, listaAtributos);
            List<ForeignKey> listaRelacionamentos = MapearRelacionamentos(sql);
            string tableName = TableName(sql);

            GerarDomain(classFolder, className, fullPath, listaAtributos, listaRelacionamentos);
            GerarIRepository(classFolder, className, domainClassFolder);
            GerarRepository(classFolder, className, dataRepositoriesFolder);
            GerarValidator(classFolder, className, domainClassFolder);
            GerarConfiguration(classFolder, className, dataConfigurationFolder, tableName, primaryKey.listaChaves, primaryKey.chavesFormatadas, listaAtributos, listaRelacionamentos);
            GerarIService(classFolder, className, applicationContractsFolder);
            GerarService(classFolder, className, applicationServicesFolder, classNameFormatada);
            GerarController(classFolder, className, controllerControllersFolder, classNameFormatada);

            AlertaFim alertaFim = new();
            alertaFim.Show();
        }

        #region Métodos SQL
        private string TableName(string sql)
        {
            Regex regexTableName = new Regex(@"create table ([A-Z]*)");
            System.Text.RegularExpressions.Match matchTableName = regexTableName.Match(sql);
            return matchTableName.Groups[1].Value;
        }
        private (string[] listaChaves, string chavesFormatadas) GerarStringPrimaryKey(string sql, List<AttributeInfo> listaAtributos)
        {
            Regex regexPrimaryKey = new Regex(@"primary key \(([A-Z_]+(?:, [A-Z_]+)*)\)");
            System.Text.RegularExpressions.Match matchPrimaryKey = regexPrimaryKey.Match(sql);
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
            return (primaryKeys, primaryKeysString);
        }
        private string ObterSQL()
        {
            InserirSQL formSql = new();
            if (formSql.ShowDialog() == DialogResult.OK)
                return formSql.textBox1.Text;
            else
                throw new Exception("Texto invalido e/ou vazio.");
        }
        #endregion

        #region Métodos grid chave estrangeira
        private List<ForeignKey> MapearRelacionamentos(string sql)
        {
            Regex regexForeignKey = new Regex(@"foreign key\s*\(\s*(.+?)\s*\)\s*references\s*([A-Z_]+)\s*\(\s*(.+?)\s*\)");
            MatchCollection matchesForeignKey = regexForeignKey.Matches(sql);
            DeParaRelacionamentosForm deParaRelacionamentosForm = PreencherDataGridRelacionamentos(matchesForeignKey);
            if (deParaRelacionamentosForm.ShowDialog() == DialogResult.OK)
                return CriarObjetoNomesAtributos(deParaRelacionamentosForm);
            else
                throw new Exception("Dados invalidos");
        }
        private DeParaRelacionamentosForm PreencherDataGridRelacionamentos(MatchCollection matchesForeignKey)
        {
            DeParaRelacionamentosForm deParaRelacionamentosForm = new();
            foreach (System.Text.RegularExpressions.Match match in matchesForeignKey)
            {
                deParaRelacionamentosForm.grid.Rows.Add(match.Groups[1].Value, match.Groups[2].Value);
            };
            return deParaRelacionamentosForm;
        }
        private List<ForeignKey> CriarObjetoNomesAtributos(DeParaRelacionamentosForm deParaRelacionamentosForm)
        {
            List<ForeignKey> listaRelacionamentos = new();
            foreach (DataGridViewRow row in deParaRelacionamentosForm.grid.Rows)
            {
                if (!string.IsNullOrEmpty(row.Cells[0].Value?.ToString()))
                {
                    ForeignKey foreignKey = new ForeignKey()
                    {
                        ReferencesRelationName = row.Cells["Classe"].Value.ToString(),
                        ReferencesSqlName = row.Cells["Atributo"].Value.ToString(),
                        ForeignType = (ForeignType)row.Cells["Cardinalidade"].RowIndex,
                    };

                    listaRelacionamentos.Add(foreignKey);
                }
            }
            return listaRelacionamentos;
        }
        #endregion

        #region Métodos grid atributos
        private List<AttributeInfo> DeParaAtributos(string sql)
        {
            Regex regexAtributos = new Regex(@"([A-Z_]+) +([A-Z_]+) *(not null)*");
            MatchCollection matchesAtributos = regexAtributos.Matches(sql);

            DeParaAtributosForm deParaAtributosForm = PreencherDataGridAtributos(matchesAtributos);

            if (deParaAtributosForm.ShowDialog() == DialogResult.OK)
            {
                Dictionary<string, string> deParaAtributos = CriarObjetoNomesAtributos(deParaAtributosForm);
                return CriarListaAtributos(deParaAtributos, matchesAtributos);
            }
            else
            {
                throw new Exception("Erro ao mapear atributos");
            }
        }
        private List<AttributeInfo> CriarListaAtributos(Dictionary<string, string> deParaAtributos, MatchCollection matchesAtributos)
        {
            List<AttributeInfo> listaAtributos = new();
            foreach (System.Text.RegularExpressions.Match match in matchesAtributos)
            {
                AttributeInfo atributo = new AttributeInfo()
                {
                    Name = deParaAtributos.FirstOrDefault(x => x.Key == match.Groups[1].Value).Value,
                    SqlName = match.Groups[1].Value,
                    Type = match.Groups[2].Value,
                    Nullable = match.Groups[3].Value != "not null"
                };
                listaAtributos.Add(atributo);
            };
            return listaAtributos;
        }
        private Dictionary<string, string> CriarObjetoNomesAtributos(DeParaAtributosForm deParaAtributosForm)
        {
            Dictionary<string, string> deParaAtributos = new();
            foreach (DataGridViewRow row in deParaAtributosForm.grid.Rows)
            {
                if (!string.IsNullOrEmpty(row.Cells[0].Value?.ToString()))
                {
                    string nomeSql = row.Cells["NomeSql"].Value.ToString();
                    string nomeCSharp = row.Cells["NomeC"].Value.ToString();
                    deParaAtributos.Add(nomeSql, nomeCSharp);
                }
            }
            return deParaAtributos;
        }
        private DeParaAtributosForm PreencherDataGridAtributos(MatchCollection matchesAtributos)
        {
            DeParaAtributosForm deParaAtributosForm = new();
            foreach (System.Text.RegularExpressions.Match match in matchesAtributos)
            {
                deParaAtributosForm.grid.Rows.Add(match.Groups[1].Value);
            };
            return deParaAtributosForm;
        }
        #endregion

        #region Métodos de escrever arquivos
        private void GerarDomain(string classFolder, string className, string fullPath, List<AttributeInfo> listaAtributos, List<ForeignKey> listaRelacionamentos)
        {
            string domainContent =
            "using FluentValidation;\n" +
            "using Sisand.Vision.Domain." + classFolder + ".Validators;\n";
            foreach (var relacionamento in listaRelacionamentos)
            {
                domainContent += "using Sisand.Vision.Domain." + relacionamento.ReferencesRelationName + ";\n";
            }
            domainContent +=
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
            "\t\t#region Relacionamentos\n";

            foreach (var relacionamento in listaRelacionamentos)
            {
                domainContent += "\t\tpublic virtual " + relacionamento.ReferencesRelationName + " " + relacionamento.ReferencesRelationName + " { get; set; }\n";
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
        }
        private void GerarIRepository(string classFolder, string className, string domainClassFolder)
        {
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
        }
        private void GerarRepository(string classFolder, string className, string dataRepositoriesFolder)
        {
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
        }
        private void GerarValidator(string classFolder, string className, string domainClassFolder)
        {
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
        }
        private void GerarConfiguration(string classFolder, string className, string dataConfigurationFolder, string tableName, string[] primaryKeys, string primaryKeysString, List<AttributeInfo> listaAtributos, List<ForeignKey> listaRelacionamentos)
        {
            string configurationFile = Path.Combine(dataConfigurationFolder, className + "Configuration.cs");
            if (!File.Exists(configurationFile))
            {
                File.Create(configurationFile).Dispose();

                string configurationContent =
                "using Microsoft.EntityFrameworkCore;\n" +
                "using Microsoft.EntityFrameworkCore.Metadata.Builders;\n" +
                "using Microsoft.EntityFrameworkCore.Storage.ValueConversion;\n" +
                "using Sisand.Vision.Domain." + classFolder + ";\n";
                foreach (var relacionamento in listaRelacionamentos)
                {
                    configurationContent += "using Sisand.Vision.Domain." + relacionamento.ReferencesRelationName + ";\n";
                }
                configurationContent +=
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

                foreach (var foreing in listaRelacionamentos)
                {
                    if (foreing.ForeignType == ForeignType.UmPraUm)
                        configurationContent += "\t\t\t\tbuilder.HasOne(p => p." + foreing.ReferencesRelationName + ") \n" +
                            "\t\t\t\t\t.WithOne(b => b." + className + ")\n";
                    else if (foreing.ForeignType == ForeignType.UmPraMuitos)
                        configurationContent += "\t\t\t\tbuilder.HasOne(p => p." + foreing.ReferencesRelationName + ") \n" +
                            "\t\t\t\t\t.WithMany(b => b." + className + ")\n";
                    else if (foreing.ForeignType == ForeignType.MuitosPraUm)
                        configurationContent += "\t\t\t\tbuilder.HasMany(p => p." + foreing.ReferencesRelationName + ") \n" +
                            "\t\t\t\t\t.WithOne(b => b." + className + ")\n";
                    else if (foreing.ForeignType == ForeignType.UmPraMuitos)
                        configurationContent += "\t\t\t\tbuilder.HasMany(p => p." + foreing.ReferencesRelationName + ") \n" +
                            "\t\t\t\t\t.WithMany(b => b." + className + ")\n";
                    var foreingKeys = foreing.ReferencesSqlName.Split(',');
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

        }
        private void GerarIService(string classFolder, string className, string applicationContractsFolder)
        {
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
        }
        private void GerarService(string classFolder, string className, string applicationServicesFolder, string classNameFormatada)
        {
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
        }
        private void GerarController(string classFolder, string className, string controllerControllersFolder, string classNameFormatada)
        {
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
        }
        #endregion
    }
}
