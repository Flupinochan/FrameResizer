import os
import argparse

def generate_wix_components(directory_path, configuration="Release"):
    # 実際のパスを構築（$(Configuration)の部分を置換）
    actual_path = directory_path.replace("$(Configuration)", configuration)
    
    # ディレクトリが存在するか確認
    if not os.path.exists(actual_path):
        print(f"エラー: 指定されたディレクトリが存在しません: {actual_path}")
        return None
    
    # ディレクトリ内のファイルを取得
    files = []
    try:
        files = [f for f in os.listdir(actual_path) if os.path.isfile(os.path.join(actual_path, f))]
    except Exception as e:
        print(f"エラー: ディレクトリの読み取り中にエラーが発生しました: {e}")
        return None
    
    if not files:
        print(f"警告: 指定されたディレクトリにファイルが見つかりませんでした: {actual_path}")
        return None
    
    # XMLの構築
    xml_lines = []
    xml_lines.append('<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">')
    xml_lines.append('  <Fragment>')
    xml_lines.append('    <ComponentGroup Id="FrameResizerComponents" Directory="INSTALLFOLDER">')
    
    # 各ファイルに対してコンポーネントを作成
    for file_name in sorted(files):
        xml_lines.append('      <Component>')
        xml_lines.append(f'        <File Source="{directory_path}\\{file_name}" />')
        xml_lines.append('      </Component>')
    
    xml_lines.append('    </ComponentGroup>')
    xml_lines.append('  </Fragment>')
    xml_lines.append('</Wix>')
    
    return '\n'.join(xml_lines)

def main():
    parser = argparse.ArgumentParser(description='WiX用のXMLコンポーネントテンプレートを生成します')
    parser.add_argument('--dir', type=str, required=True, help='ファイルを取得するディレクトリパス')
    parser.add_argument('--config', type=str, default="Release", help='ビルド設定（デフォルト: Release）')
    parser.add_argument('--output', type=str, help='出力ファイル（省略時は標準出力）')
    
    args = parser.parse_args()
    
    xml_content = generate_wix_components(args.dir, args.config)
    
    if xml_content:
        if args.output:
            with open(args.output, 'w', encoding='utf-8') as f:
                f.write(xml_content)
            print(f"XMLテンプレートを {args.output} に保存しました")
        else:
            print(xml_content)

if __name__ == "__main__":
    main()

# 標準出力に出力する場合
# python generate_wix_components.py --dir "..\FrameResizer\bin\$(Configuration)\net8.0-windows" --config "Release"

# ファイルに出力する場合
# python generate_wix_components.py --dir "..\FrameResizer\bin\$(Configuration)\net8.0-windows" --config "Release" --output "FrameResizerComponents.wxs"