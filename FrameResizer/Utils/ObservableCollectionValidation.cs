using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace FrameResizer.Utils;

// https://learn.microsoft.com/ja-jp/dotnet/communitytoolkit/mvvm/observablevalidator#custom-validation-methods
public class ObservableCollectionValidation : ValidationAttribute
{
    public Int32 MinCount { get; }
    public String ErrMsg { get; }

    public ObservableCollectionValidation(Int32 minCount, String errMsg)
    {
        this.MinCount = minCount;
        this.ErrMsg = errMsg;
    }

    protected override ValidationResult IsValid(Object value, ValidationContext validationContext)
    {
        // valueにはバリデーション対象の変数が代入
        if(value is ICollection collection)
        {
            if(collection.Count >= this.MinCount)
            {
                // 成功
                return ValidationResult.Success;
            }
        }
        // 失敗
        return new(this.ErrMsg);
    }
}
