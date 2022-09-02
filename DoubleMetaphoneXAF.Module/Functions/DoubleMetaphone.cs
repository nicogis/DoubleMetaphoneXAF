using DevExpress.Data.Filtering;
using DevExpress.Xpo.DB;
using System;

namespace DoubleMetaphoneXAF.Module.Functions
{

    public class DoubleMetaphone : ICustomFunctionOperatorFormattable
    {
        public string Name
        {
            get
            {
                return "DoubleMetaphone";
            }
        }
        public object Evaluate(params object[] operands)
        {
            throw new NotImplementedException();
        }
        public Type ResultType(params Type[] operands)
        {
            return typeof(object);
        }
        static DoubleMetaphone()
        {
            DoubleMetaphone instance = new DoubleMetaphone();
            if (CriteriaOperator.GetCustomFunction(instance.Name) == null)
            {
                CriteriaOperator.RegisterCustomFunction(instance);
            }
        }
        public static void Register()
        {
        }

        public string Format(Type providerType, params string[] operands)
        {
            if (providerType == typeof(MSSqlConnectionProvider))
            {
               
                return $"dbo.udf_double_metaphone({operands[0]}) = {operands[1]}";
            }
                
            throw new NotSupportedException($"This provider is not supported: {providerType.Name}");           
        }
    }

}
