# Double Metaphone XAF

This is a prof of concepts to use [DoubleMetaphone](https://en.wikipedia.org/wiki/Metaphone) algorithm in XAF

- Build DoubleMetaphone project

- Register [dll in SQL Server](https://docs.microsoft.com/en-us/sql/relational-databases/clr-integration/database-objects/getting-started-with-clr-integration?view=sql-server-ver16)

  Can follow these steps (here I use Northwind):
    

```<language>
        ALTER DATABASE Northwind
        SET TRUSTWORTHY ON;

        CREATE ASSEMBLY DoubleMetaphone
        FROM 'C:\Temp\DoubleMetaphone.dll' WITH PERMISSION_SET = SAFE

        ALTER DATABASE Northwind
        SET TRUSTWORTHY OFF;
```

Visualize list of assembly in db
```<language>
    SELECT * from sys.assemblies
```

From Assembly select DoubleMetaphone and create script from assembly in clipboard
![create script from assembly](https://github.com/nicogis/DoubleMetaphoneXAF/blob/master/DoubleMetaphone/images/createAssembly.png)
Clipboard:

```<language>
CREATE ASSEMBLY [DoubleMetaphone]
FROM 0x4D5A90...
WITH PERMISSION_SET = SAFE
GO
```

Copy 0x4D5A90... in variable @asmBin and run this script to add assembly in trusted assembly

```<language>
USE master;
GO
DECLARE @clrName nvarchar(4000) = 'doublemetaphone, version=0.0.0.0, culture=neutral, publickeytoken=null, processorarchitecture=msil'
DECLARE @asmBin varbinary(max) = 0x4D5A90...;
DECLARE @hash varbinary(64);

SELECT @hash = HASHBYTES('SHA2_512', @asmBin);

EXEC sys.sp_add_trusted_assembly @hash = @hash,
                                 @description = @clrName;
```

Visualize list of assembly trusted
```<language>
select * FROM sys.trusted_assemblies
```

Create function in db

```<language>
CREATE FUNCTION dbo.udf_double_metaphone(@value nvarchar(max)) 
RETURNS nvarchar(max)   
AS EXTERNAL NAME DoubleMetaphone.DoubleMetaphoneUDT.DoubleMetaphoneExec;   
GO 
```

Enable crl
```<language>

EXEC sp_configure 'clr enabled', 1;  
RECONFIGURE;  
GO 
```


In project XAF agnostic module create a custom function

```<language>
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
```

In module.cs register the custom function

```<language>
public sealed partial class DoubleMetaphoneXAFModule : ModuleBase {
        public DoubleMetaphoneXAFModule() {
            InitializeComponent();
            Functions.DoubleMetaphone.Register();
            ....
        }
        
    }
```

Add a controller to custom search standard 
```<language>
public partial class VCSearchDoubleMetaphone : ViewController
    {
        // Use CodeRush to create Controllers and Actions with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/403133/
        public VCSearchDoubleMetaphone()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
            TargetObjectType = typeof(Customers);
        }
        private FilterController standardFilterController;
        protected override void OnActivated()
        {
            base.OnActivated();
            standardFilterController = Frame.GetController<FilterController>();
            if (standardFilterController != null)
            {
                
                standardFilterController.CustomBuildCriteria += StandardFilterController_CustomBuildCriteria;
            }
        }

        private void StandardFilterController_CustomBuildCriteria(object sender, CustomBuildCriteriaEventArgs e)
        {
            if (string.IsNullOrEmpty(e.SearchText))
            {
                e.Handled = false;
                return;
            }
            
            e.Criteria = CriteriaOperator.Parse($"DoubleMetaphone('{e.SearchText}', [CompanyNameDoubleMetaphone]) or CharIndex('{e.SearchText}', [CompanyName]) > -1");
            e.Handled = true;    
        }

        
        protected override void OnDeactivated()
        {
            if (standardFilterController != null)
            {
                standardFilterController.CustomBuildCriteria -= StandardFilterController_CustomBuildCriteria;
            }
            base.OnDeactivated();
        }
    }
```

In db Northwind add in table Customers the field CompanyNameDoubleMetaphone

```<language>
USE [Northwind]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE Customers
ADD [CompanyNameDoubleMetaphone]  AS ([dbo].[udf_double_metaphone]([CompanyName])) PERSISTED
GO
```


In table customers I have add three similar strings (CompanyName field)
<pre>'Blauer See    Delikatessen sa'
'Blauer Se    Delikatessen s.a.'
'Blaer Se    Deliktesse s.a.'</pre>

and I search for example ***'Blaer Se Delikatessen'*** I match all strings

![search](https://github.com/nicogis/DoubleMetaphoneXAF/blob/master/DoubleMetaphone/images/search.png)







     





 
