tableextension 50002 "Purchase Header Extension" extends "Purchase Header"
{
    fields
    {
        field(50001; "Custom Reference No."; Code[30])
        {
            Caption = 'Custom Reference No.';
        }
        field(50002; "Approval Status"; Enum "Approval Status")
        {
            Caption = 'Approval Status';
        }
    }
}
