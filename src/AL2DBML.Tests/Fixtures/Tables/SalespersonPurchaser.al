table 13 "Salesperson/Purchaser"
{
    Caption = 'Salesperson/Purchaser';
    DataClassification = ToBeClassified;

    fields
    {
        field(1; "Code"; Code[10])
        {
            Caption = 'Code';
        }
        field(2; "Name"; Text[50])
        {
            Caption = 'Name';
        }
        field(3; "Commission %"; Decimal)
        {
            Caption = 'Commission %';
        }
        field(4; "E-Mail"; Text[80])
        {
            Caption = 'E-Mail';
        }
        field(5; "Salesperson/Purchaser Type"; Enum "Salesperson/Purchaser Type")
        {
            Caption = 'Salesperson/Purchaser Type';
        }
    }

    keys
    {
        key(PK; "Code")
        {
            Clustered = true;
        }
        key(SK1; "Name")
        {
        }
    }
}
