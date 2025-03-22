// https://learn.microsoft.com/ja-jp/dotnet/communitytoolkit/mvvm/generators/observableproperty
// https://learn.microsoft.com/ja-jp/dotnet/communitytoolkit/mvvm/observablevalidator
// https://learn.microsoft.com/ja-jp/dotne
// https://learn.microsoft.com/ja-jp/dotnet/api/system.windows.data.updatesourcetrigger?view=windowsdesktop-9.0
// https://learn.microsoft.com/ja-jp/uwp/api/windows.ui.xaml.data.bindingmode?view=winrt-26100

// ViewModel用 継承クラス
/// 1. ObservableObject
/// 2. ObservableValidator (ObservableObjectの派生クラス)
/// 3. ObservableRecipient (ObservableObjectの派生クラス)

/// 1. public partial class クラス名 : ObservableObject
///    ViewModel用クラスは、partialにして、ObservableObjectを割り当てる => XAMLとのEvent通知が可能
///    バリデーション機能がついたObservableValidatorか、ViewModel間のデータやりとり機能がついたObservableRecipientを使用することになる

/// 2. public partial class クラス名 : ObservableValidator
///    ObservableObjectの変数変更時のPropertyChangedイベントに加え、
///    バリデーションエラー時のErrorsChangedイベントも発生させる派生クラス

/// 3. public partial class クラス名 : ObservableRecipient
///    ObservableValidator と競合し、多重継承はできない
///    ObservableRecipient は、IMessenger を利用するために最適化されているクラス
///    IMessenger を利用して、異なる ViewModel 間でメッセージを送受信する場合に必要
///    基本的には、バリデーション機能がある ObservableValidator を使用すればよい

// データ送信 (バインド)
/// 1. [RelayCommand]
/// 2. [INotifyPropertyChanged]
/// 3. [IMessenger]
/// 4. UpdateSourceTrigger
/// 5. Mode 

/// 1. XAML からのアクション(ボタンのクリック等)を ViewModel に伝えるために使用
///    clickイベントは、XAML Button CommandプロパティでViewModelの関数を指定すればOK
///    click等のイベントは、MainWindow等のWindowの関数を呼び出すが、RelayCommandは、他のクラス(ViewModel)の関数を呼び出し可能
///    click以外のイベント、textbox textchangedイベント等の場合は、MainWindow等の関数を通常どおり呼び出し、
///    viewModel.OnTextChangedCommand.Execute(textBox.Text); とすることで、ViewModelのOnTextChangedCommand関数を呼び出せる
///    CanExecute で有効/無効を切り替えられる
///    AllowConcurrentExecutions で非同期 同時実行が可能
///  ★RelayCommandを利用することで、MainWindow等に処理を記載せず、ViewModel側に処理を記載可能になる
/// 2. ViewModel のプロパティが変更された際に、その変更を XAML に伝えるために使用
///    [ObservableProperty] を付けたプロパティの大文字の変数をXAMLで使用すれば、プロパティ変更時に自動的にUIが更新される
/// 3. ViewModel 間でのメッセージ送信や受信に使用
/// 4. textBoxなどの入力系のコントロールで指定し、XAMLからViewModelへの反映タイミングを指定
///    PropertyChanged を指定することで、リアルタイムにViewModelに反映可能
///    LostFocus でフォーカスが外れたときに反映
///    ※ViewModelからXAMLへの反映タイミングは、ViewModelのフィールドの値が変更されたとき
/// 5. XAML TwoWay で双方向バインド
///                      Text="{Binding Path=OutputFolderPath,
///                                     UpdateSourceTrigger = PropertyChanged,
///                                     Mode = TwoWay,
///                                     FallbackValue = バインドされていません,
///                                     TargetNullValue = バインド対象がNullです}"

// [ObservableProperty]
/// 設定された変数が変更された場合に、PropertyChanged イベントを発火させる
/// 自動的に発火されるOn変数名Changed メソッドは、PropertyChanged イベントを発火時に実行されるメソッド
/// フィールドは小文字に設定し、そのフィールドにアクセスする際は大文字でアクセスする
/// ObservableCollection バインド用のList

// [NotifyPropertyChangedFor(nameof(変数名))]
/// 指定した変数のPropertyChanged イベントも発火させて、値を渡せる

// [NotifyCanExecuteChangedFor(nameof(コマンド名))]
/// 指定した関数(コマンド)のCanExecute(有効/無効)を自動更新する設定
/// 通常は、以下の手順で有効/無効を切り替えるが、1の手順だけでOKになる
/// 1.  _canExecute = true/false を設定
/// 2. MyCommand.NotifyCanExecuteChanged() でUIに反映

// バリデーション (エラー処理)
/// Submitボタンクリック時などで、バリデーションは手動実行する必要がある
/// 1. [Required(ErrorMessage = "メッセージ")] バリデーションエラー時に指定したエラーメッセージを渡す
/// 2. ValidateAllProperties() 手動で全変数にバリデーションを実行
/// 3. ValidateProperty(nameof(変数名)); 手動で指定した変数にバリデーションを実行
/// 4. TrySetProperty バリデーション成功時のみ値をセットしたい場合 (通常は値がセットされたうえで、バリデーションエラーが起きる)
/// 5. ClearAllErrors エラーリセット
/// 6. HasErrors オブジェクトがエラーを持っているかどうかを示すプロパティ エラーが1つでも存在する場合は true を返す
/// 7. GetErrors (変数名) メソッドは引数の変数にエラーがある場合は、エラーメッセージのリストを返す

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrameResizer.Interface;
using FrameResizer.Utils;
using Microsoft.WindowsAPICodePack.Dialogs;
using SixLabors.ImageSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;

namespace FrameResizer.ViewModel;

public partial class ImageConfig : ObservableValidator
{
    // ソースフォルダ名
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OutputFolderPath))]
    [Required(ErrorMessage = "Source folder path is required.")]
    private String _sourceFolderPath = String.Empty;

    // 出力フォルダ名
    [ObservableProperty]
    [Required(ErrorMessage = "Output folder path is required.")]
    private String _outputFolderPath = String.Empty;

    // 画像ファイル名のList(ObservableCollection)
    [ObservableProperty]
    [ObservableCollectionValidation(1, "must be one element")]
    private ObservableCollection<String> _selectedImageNameList = new ObservableCollection<String>();

    // Width
    [ObservableProperty]
    private Int32 _outputWidth = 708;

    // Height
    [ObservableProperty]
    private Int32 _outputHeight = 1;

    // Border Color
    [ObservableProperty]
    [Required(ErrorMessage = "Color code is required.")]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Invalid color code.")]
    private String _borderColor = "#444444";

    // Border Size
    [ObservableProperty]
    [Range(1, Int32.MaxValue, ErrorMessage = "The border size must be at least 1px.")]
    private Int32 _borderSize = 1;

    // Width Radio Buttonの有効/無効状態
    [ObservableProperty]
    private Boolean _isWidthDecimalUpDownEnabled = true;

    // Height Radio Buttonの有効/無効状態
    [ObservableProperty]
    private Boolean _isHeightDecimalUpDownEnabled = false;

    // ComboBox
    [ObservableProperty]
    private ObservableCollection<String> _processType = new ObservableCollection<String>() { "リサイズ + 枠線", "リサイズのみ", "枠線のみ" };
    [ObservableProperty]
    private String _selectedProcessType = "リサイズ + 枠線";

    // ダイアログ
    [ObservableProperty]
    private Boolean _isDialogOpen = false;
    [ObservableProperty]
    private String _dialogMessage = String.Empty;

    // 実行Button
    [ObservableProperty]
    private String _executeButtonContent = "実行";
    [ObservableProperty]
    private Boolean _executeButtonIsEnabled = true;
    [ObservableProperty]
    private Boolean _isIndeterminate = false;
    [ObservableProperty]
    private Boolean _isIndicatorVisible = false;

    private readonly ICustomiseImage _customiseImage;

    public ImageConfig(ICustomiseImage customiseImage)
    {
        this._customiseImage = customiseImage;
        // PropertyChangedイベントのOn変数名Changed メソッドは自動生成されるが、
        // エラーイベント用の関数は以下のように手動で設定する必要がある
        this.ErrorsChanged += OnErrorsChanged!;
    }

    // ソースフォルダ名を出力フォルダ名に同期
    partial void OnSourceFolderPathChanged(string value)
    {
        OutputFolderPath = value;
    }

    // エラー時に実行される関数 ※未使用
    private void OnErrorsChanged(Object sender, DataErrorsChangedEventArgs e)
    {
        if (e.PropertyName is not null)
        {
            String errorPropertyName = e.PropertyName;
        }
    }


    ///////////////
    /// Command ///
    ///////////////
    // ファイル選択ボタン
    // ソースフォルダ、出力フォルダ、画像ファイルを取得
    [RelayCommand]
    public void SelectFileButtonClick()
    {
        try
        {
            CommonOpenFileDialog fileDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = false, // ファイル選択モード
                Multiselect = true,     // 複数選択を許可
                Title = "ファイルを選択してください ※複数選択可",
            };
            fileDialog.Filters.Add(new CommonFileDialogFilter("Images", "*.jpg;*.jpeg;*.png"));
            fileDialog.Filters.Add(new CommonFileDialogFilter("All", "*.*"));

            CommonFileDialogResult fileDialogResult = fileDialog.ShowDialog();
            if(fileDialogResult is CommonFileDialogResult.Cancel or CommonFileDialogResult.None)
                return;
            if(fileDialog.FileNames is null)
                return;

            this.SourceFolderPath = Path.GetDirectoryName(fileDialog.FileNames.First())!;
            this.OutputFolderPath = Path.GetDirectoryName(fileDialog.FileNames.First())!;
            this.SelectedImageNameList.Clear();
            foreach(String filePath in fileDialog.FileNames)
            {
                this.SelectedImageNameList.Add(Path.GetFileName(filePath));
            }
        }
        catch(Exception ex)
        {
            this.DialogMessage = "エラーが発生しました";
            this.IsDialogOpen = true;
        }

    }

    // フォルダ選択ボタン
    // ソースフォルダ、出力フォルダ、画像ファイルを取得
    [RelayCommand]
    public void SelectFolderButtonClick()
    {
        try
        {
            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = false,
                Title = "フォルダを選択してください"
            };

            CommonFileDialogResult fileDialogResult = folderDialog.ShowDialog();
            if(fileDialogResult is CommonFileDialogResult.Cancel or CommonFileDialogResult.None)
                return;
            if(folderDialog.FileNames is null)
                return;

            String[] imageExtensions = { ".jpg", ".jpeg", ".png" };
            String[] selectedFilesPath = Directory.GetFiles(folderDialog.FileName)
                                                  .Where(filePath => imageExtensions.Contains(Path.GetExtension(filePath)))
                                                  .ToArray();
            this.SourceFolderPath = folderDialog.FileName;
            this.OutputFolderPath = folderDialog.FileName;
            this.SelectedImageNameList.Clear();
            foreach(String filePath in selectedFilesPath)
            {
                this.SelectedImageNameList.Add(Path.GetFileName(filePath));
            }
        }
        catch(Exception ex)
        {
            this.DialogMessage = "エラーが発生しました";
            this.IsDialogOpen = true;
        }
    }

    // 出力フォルダ選択ボタン
    // 出力フォルダ名を取得
    [RelayCommand]
    public void SelectOutputFolderButtonClick()
    {
        try
        {
            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = false,
                Title = "出力フォルダを選択してください"
            };

            CommonFileDialogResult fileDialogResult = folderDialog.ShowDialog();
            if(fileDialogResult is CommonFileDialogResult.Cancel or CommonFileDialogResult.None)
                return;
            if(folderDialog.FileNames is null)
                return;

            this.OutputFolderPath = folderDialog.FileName;
        }
        catch(Exception ex)
        {
            this.DialogMessage = "エラーが発生しました";
            this.IsDialogOpen = true;
        }
    }

    // Widthラジオボタン
    [RelayCommand]
    public void WidthRadioButtonChecked()
    {
        this.IsWidthDecimalUpDownEnabled = true;
        this.IsHeightDecimalUpDownEnabled = false;
        this.OutputHeight = 1;
    }

    // Heightラジオボタン
    [RelayCommand]
    public void HeightRadioButtonChecked()
    {
        this.IsWidthDecimalUpDownEnabled = false;
        this.IsHeightDecimalUpDownEnabled = true;
        this.OutputWidth = 1;
    }

    // Dialogを閉じるButton
    [RelayCommand]
    public void CloseDialogButtonClick() => this.IsDialogOpen = false;

    /// 実行 Button
    [RelayCommand]
    async public Task ExecuteButtonClick()
    {
        // Button無効化/ローディング開始
        this.ExecuteButtonContent = "処理中";
        this.IsIndeterminate = true;
        this.IsIndicatorVisible = true;
        this.ExecuteButtonIsEnabled = false;

        // バリデーション実行
        ValidateAllProperties();

        if(!this.HasErrors)
        {
            await Task.Delay(1000);

            // 画像処理
            try
            {
                Color color = Color.ParseHex(this.BorderColor);

                List<String> sourceImagePaths = new List<String>();
                List<String> outputImagePaths = new List<String>();
                foreach(String imageName in this.SelectedImageNameList)
                {
                    // ソースイメージの絶対パス
                    String sourceImagePath = Path.Combine(this.SourceFolderPath, imageName);
                    sourceImagePaths.Add(sourceImagePath);
                    // 出力イメージの絶対パス
                    String outputImagePath = Path.Combine(this.OutputFolderPath, imageName);
                    outputImagePaths.Add(outputImagePath);
                }

                ParallelLoopResult parallelLoopResult = Parallel.For(0, sourceImagePaths.Count, i =>
                {
                    String sourceImagePath = sourceImagePaths[i];
                    String outputImagePath = outputImagePaths[i];

                    if(this.SelectedProcessType.Equals("リサイズ + 枠線"))
                    {
                        _customiseImage.Convert(sourceImagePath, outputImagePath,
                        this.OutputHeight, this.OutputWidth,
                        this.BorderSize, color);
                    }
                    else if(this.SelectedProcessType.Equals("リサイズのみ"))
                    {
                        _customiseImage.Convert(sourceImagePath, outputImagePath,
                        this.OutputHeight, this.OutputWidth);
                    }
                    else if(this.SelectedProcessType.Equals("枠線のみ"))
                    {
                        _customiseImage.Convert(sourceImagePath, outputImagePath,
                        this.BorderSize, color);
                    }
                });

                if(parallelLoopResult.IsCompleted)
                    this.DialogMessage = "処理が正常に完了しました";
            }
            catch(Exception ex)
            {
                this.DialogMessage = "エラーが発生しました";
            }
        }
        else
        {
            this.DialogMessage = "エラーが発生しました";
        }

        // Button有効化/ローディング終了
        this.ExecuteButtonContent = "実行";
        this.IsIndeterminate = false;
        this.IsIndicatorVisible = false;
        this.ExecuteButtonIsEnabled = true;
        // ダイアログ表示
        this.IsDialogOpen = true;
    }
}