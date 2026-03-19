tableextension 50001 "Salesperson/Purchaser Extension" extends "Salesperson/Purchaser"
{
    fields
    {
        field(50001; "Phone No."; Text[30])
        {
            Caption = 'Phone No.';
        }
        field(50002; "Global Dimension 1 Code"; Code[20])
        {
            Caption = 'Global Dimension 1 Code';
        }
    }
}
