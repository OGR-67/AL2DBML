table 81 "Gen. Journal Line"
{
    Caption = 'Gen. Journal Line';
    DataClassification = ToBeClassified;

    fields
    {
        field(1; "Journal Template Name"; Code[10])
        {
            Caption = 'Journal Template Name';
        }
        field(2; "Line No."; Integer)
        {
            Caption = 'Line No.';
        }
        field(3; "Account Type"; Enum "Gen. Journal Account Type")
        {
            Caption = 'Account Type';
        }
        field(4; "Account No."; Code[20])
        {
            Caption = 'Account No.';
        }
        field(5; "Document Type"; Enum "Gen. Journal Document Type")
        {
            Caption = 'Document Type';
        }
        field(6; "Amount"; Decimal)
        {
            Caption = 'Amount';
        }
        field(7; "VAT Amount"; Decimal)
        {
            Caption = 'VAT Amount';
        }
        field(8; "Bal. Account No."; Code[20])
        {
            Caption = 'Bal. Account No.';
        }
    }

    keys
    {
        key(PK; "Journal Template Name", "Line No.")
        {
            Clustered = true;
        }
    }
}
