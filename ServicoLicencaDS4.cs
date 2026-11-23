using System;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;

namespace ObysidianDS4
{
    public class RegistroLicencaDS4
    {
        public string Chave { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Vitalícia"; // "Vitalícia" ou "Mensal"
        public int DiasValidade { get; set; } = 0;
        public bool Ativada { get; set; } = false;
        public string? HwidVinculado { get; set; }
        public DateTime? DataAtivacao { get; set; }
        public DateTime? DataExpiracao { get; set; }
    }

    public class ServicoLicencaDS4
    {
        private static readonly string CaminhoArquivoLicencaLocal = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DS4Windows",
            "licenca_ds4.dat"
        );

        public static string ObterHWID()
        {
            try
            {
                string cpuId = "";
                string biosId = "";

                using (var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (var item in searcher.Get())
                    {
                        cpuId = item["ProcessorId"]?.ToString() ?? "";
                        break;
                    }
                }

                using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS"))
                {
                    foreach (var item in searcher.Get())
                    {
                        biosId = item["SerialNumber"]?.ToString() ?? "";
                        break;
                    }
                }

                if (string.IsNullOrEmpty(cpuId) && string.IsNullOrEmpty(biosId))
                {
                    using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
                    biosId = key?.GetValue("MachineGuid")?.ToString() ?? Environment.MachineName;
                }

                string dadosBrutos = $"{cpuId}_{biosId}_{Environment.MachineName}_DS4";
                using var sha256 = SHA256.Create();
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(dadosBrutos));
                return BitConverter.ToString(hash).Replace("-", "").Substring(0, 16);
            }
            catch
            {
                return "HWID-DS4-" + Environment.MachineName.GetHashCode().ToString("X8");
            }
        }

        public bool VerificarLicencaAtiva(out string tipoLicenca, out int diasRestantes)
        {
            tipoLicenca = "Sem Licença";
            diasRestantes = 0;

            try
            {
                if (!File.Exists(CaminhoArquivoLicencaLocal))
                    return false;

                string conteudoCriptografado = File.ReadAllText(CaminhoArquivoLicencaLocal);
                string conteudo = DescriptografarTexto(conteudoCriptografado);
                var dados = JsonSerializer.Deserialize<RegistroLicencaDS4>(conteudo);

                if (dados == null || string.IsNullOrEmpty(dados.Chave))
                    return false;

                string hwidAtual = ObterHWID();
                if (dados.HwidVinculado != hwidAtual)
                    return false;

                if (dados.Tipo == "Vitalícia" || dados.Chave.StartsWith("OBS-DS4-LIFE-", StringComparison.OrdinalIgnoreCase))
                {
                    tipoLicenca = "Vitalícia PRO";
                    diasRestantes = 9999;
                    return true;
                }

                if (dados.DataExpiracao.HasValue)
                {
                    var tempoRestante = dados.DataExpiracao.Value - DateTime.Now;
                    if (tempoRestante.TotalDays > 0)
                    {
                        tipoLicenca = "Mensal (30 Dias)";
                        diasRestantes = (int)Math.Ceiling(tempoRestante.TotalDays);
                        return true;
                    }
                    else
                    {
                        tipoLicenca = "Expirada";
                        return false;
                    }
                }
            }
            catch { }

            return false;
        }

        public bool AtivarChave(string chaveDigitada, out string mensagem)
        {
            chaveDigitada = chaveDigitada.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(chaveDigitada))
            {
                mensagem = "Por favor, digite uma chave de licença válida para o Obysidian DS4.";
                return false;
            }

            bool ehVitalicia = chaveDigitada.StartsWith("OBS-DS4-LIFE-");
            bool ehMensal = chaveDigitada.StartsWith("OBS-DS4-M30-");

            if (!ehVitalicia && !ehMensal)
            {
                mensagem = "Formato de chave inválido! Chaves do Obysidian DS4 começam com OBS-DS4-LIFE- ou OBS-DS4-M30-.";
                return false;
            }

            var partes = chaveDigitada.Split('-');
            if (partes.Length < 4)
            {
                mensagem = "Chave incompleta ou inválida.";
                return false;
            }

            try
            {
                string hwid = ObterHWID();
                var registro = new RegistroLicencaDS4
                {
                    Chave = chaveDigitada,
                    Tipo = ehVitalicia ? "Vitalícia" : "Mensal",
                    DiasValidade = ehVitalicia ? 0 : 30,
                    Ativada = true,
                    HwidVinculado = hwid,
                    DataAtivacao = DateTime.Now,
                    DataExpiracao = ehVitalicia ? null : DateTime.Now.AddDays(30)
                };

                string json = JsonSerializer.Serialize(registro);
                string criptografado = CriptografarTexto(json);

                string? pasta = Path.GetDirectoryName(CaminhoArquivoLicencaLocal);
                if (!string.IsNullOrEmpty(pasta))
                    Directory.CreateDirectory(pasta);

                File.WriteAllText(CaminhoArquivoLicencaLocal, criptografado);

                mensagem = ehVitalicia 
                    ? "Licença VITALÍCIA do Obysidian DS4 ativada com sucesso!" 
                    : "Licença MENSAL (30 Dias) do Obysidian DS4 ativada com sucesso!";
                return true;
            }
            catch (Exception ex)
            {
                mensagem = "Erro ao registrar licença: " + ex.Message;
                return false;
            }
        }

        private static string CriptografarTexto(string texto)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(texto);
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)(bytes[i] ^ 0x6C);
            return Convert.ToBase64String(bytes);
        }

        private static string DescriptografarTexto(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)(bytes[i] ^ 0x6C);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
