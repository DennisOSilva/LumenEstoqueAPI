using System.ComponentModel.DataAnnotations;

namespace LumenEstoque.Validations;

public class CnpjAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        var cnpj = value.ToString()!.Replace(".", "").Replace("/", "").Replace("-", "");

        if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
            return ValidationResult.Success;

        if (cnpj.Length != 14 || cnpj.Distinct().Count() == 1)
            return new ValidationResult("CNPJ inválido");

        if (!IsValidCnpj(cnpj))
            return new ValidationResult("CNPJ inválido");

        return ValidationResult.Success;
    }

    private static bool IsValidCnpj(string cnpj)
    {
        int[] multipliers1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multipliers2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        var sum = 0;
        for (int i = 0; i < 12; i++)
            sum += int.Parse(cnpj[i].ToString()) * multipliers1[i];

        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;

        sum = 0;
        for (int i = 0; i < 13; i++)
            sum += int.Parse(cnpj[i].ToString()) * multipliers2[i];

        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;

        return cnpj[12] == char.Parse(digit1.ToString()) &&
               cnpj[13] == char.Parse(digit2.ToString());
    }
}