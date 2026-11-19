# EzCare — Bản hoàn chỉnh, một database

Chức năng đối chiếu FR01–FR12 và R01–R06 trong bốn chương:
https://docs.google.com/document/d/13x9PucgCJ9PIafoTrk5TosIUIAtHEm7HVKxgzj4o7Ag/edit

Dự án `D:\NhapMonCNPM\EzCareProject` chỉ tham khảo cách chia trang ASPX.

## 1. Chạy trên máy mới

1. Giải nén toàn bộ ZIP, mở **EzCare.sln** trong Visual Studio 2022. Cần workload **ASP.NET and web development**, **.NET Framework 4.8 Developer Pack** và SQL Server/SQL Server Express.
2. Trong SSMS, chạy **Database/Setup.sql** một lần nếu chưa có database. Script tạo **một database EzCare**, 11 bảng, các view, 13 chuyên khoa, 26 bác sĩ và 3 tài khoản admin/tiếp nhận/bệnh nhân. Có sẵn ca khám trong 14 ngày tiếp theo, không cần chạy thêm file để có các bác sĩ mới.
3. Sửa Data Source trong **GUI/Web.config** trùng Server name trong SSMS; giữ Initial Catalog=EzCare.
4. Nhấp phải **EzCare.GUI → Set as Startup Project**; nhấp phải **Default.aspx → Set as Start Page**.
5. Build Solution rồi **F5** hoặc **Ctrl+F5** với IIS Express. Cổng mặc định **5076**.

```xml
<add name="EzCare"
     connectionString="Data Source=.\SQLEXPRESS01;Initial Catalog=EzCare;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=5"
     providerName="System.Data.SqlClient" />
```

Chạy Setup.sql chưa đủ để mở web: máy nhận vẫn cần công cụ, connection string đúng và build GUI. Không mở trực tiếp file ASPX trong trình duyệt. Nếu Visual Studio đang mở bản cũ, chọn Reload hoặc đóng/mở lại solution.

## 2. Chỉ dùng một database

Gói này chỉ có **Database/Setup.sql**, tạo duy nhất database **EzCare**. Không có database kiểm thử, file nâng cấp hoặc script dữ liệu phụ. Setup.sql đã chứa đủ bảng, view, tài khoản, 13 chuyên khoa, 26 bác sĩ và ca khám.

- **Máy mới chưa có EzCare:** chạy Setup.sql một lần rồi sửa Server name trong GUI/Web.config.
- **Máy đang chạy bản hiện tại:** giữ nguyên database EzCare và connection string; chỉ cập nhật source rồi Build. Không cần nhập lại dữ liệu.
- Không chạy lại Setup.sql trên database đã có bảng. Gói này không xóa hoặc thay thế database đang có.

## 3. Tài khoản và mật khẩu

Mật khẩu chung của các tài khoản do Setup tạo: **EzCare@123**. Thông tin bác sĩ là dữ liệu minh họa, không phải danh bạ bác sĩ đã xác minh.

| Vai trò | Tên đăng nhập | Trang sau đăng nhập |
|---|---|---|
| Quản trị | admin | QuanLyCa.aspx |
| Bác sĩ | doctor, doctor2 ... doctor26 | CaLamViec.aspx |
| Tiếp nhận | reception | TiepNhan.aspx |
| Bệnh nhân | patient | DatLich.aspx |

13 chuyên khoa: Nội tổng quát, Tai Mũi Họng, Da liễu, Tim mạch, Tiêu hóa, Hô hấp, Nội tiết, Thần kinh, Cơ xương khớp, Nhi khoa, Sản phụ khoa, Mắt và Răng Hàm Mặt. Mỗi chuyên khoa có hai bác sĩ với học vị, số năm kinh nghiệm và giới thiệu.

Setup.sql tạo hồ sơ và ca mẫu trong 14 ngày tính từ ngày khởi tạo. Các ca bổ sung luân phiên sáng/chiều, bỏ qua Chủ nhật; mỗi ca dài ba giờ, có sáu lượt 30 phút. Khi hết ngày mẫu, admin vào Ca làm việc để mở ca mới.

Trang chủ giới thiệu sáu chuyên khoa và sáu bác sĩ. Xem danh sách đầy đủ và lọc theo chuyên khoa/tên ở **Bác sĩ**; ô chọn chuyên khoa vẫn có đủ 13 mục.

Admin có các mục **Đặt lịch hộ**, **Lịch hẹn và tiếp nhận**, **Lịch cần dời** trong menu. Admin tìm/tạo hồ sơ khách, đặt hộ, xem mọi phiếu, đổi/hủy đúng hạn, ghi đã đến/không đến đúng điều kiện, liên hệ dời lịch và xuất CSV. Các thay đổi ghi đúng tài khoản admin trong lịch sử; không đổi vai trò thành nhân viên tiếp nhận khi thao tác.

Đường dẫn DatLich.aspx tự chuyển admin/tiếp nhận sang DatLichHo.aspx, giữ bác sĩ/ngày/bộ lọc. Admin mở LichHen.aspx, DanhSachKham.aspx hoặc ThongBao.aspx sẽ tới TiepNhan.aspx; CaLamViec.aspx tới QuanLyCa.aspx; DonDoiCa.aspx tới DuyetDon.aspx. Đây là các màn quản lý tương ứng, không dùng hồ sơ bệnh nhân hoặc ca cá nhân giả cho admin.

Nhấp tên người dùng ở header để mở **Hồ sơ cá nhân**. Cả bốn vai trò cập nhật thông tin tài khoản và đổi mật khẩu của mình; riêng bệnh nhân có thêm ngày sinh/giới tính/địa chỉ. Thao tác hoàn thành lượt khám và gửi đơn nghỉ vẫn thuộc bác sĩ được phân công theo R05/FR11.

Theo yêu cầu bổ sung cho bài tập lớn, **mọi tài khoản mới đều lưu mật khẩu đọc được ở dbo.Password**, đồng thời vẫn lưu PasswordHash ở Users để xác thực. Đăng ký, đổi hoặc đặt lại mật khẩu cập nhật cả hai trong cùng transaction.

Đăng nhập **admin → Mật khẩu tài khoản**, hoặc tra trong SSMS:

Hai trang **Tài khoản nhân sự** và **Mật khẩu tài khoản** có tìm theo họ tên, tên đăng nhập, số điện thoại hoặc email; hỗ trợ tên tiếng Việt không dấu. Chọn vai trò/trạng thái rồi bấm Tìm kiếm. Danh sách phân trang ngay tại SQL, mặc định 20 dòng, có thể chọn 50/100. Chuyển trang hoặc mở/lưu tài khoản giữ bộ lọc đang dùng. Xóa bộ lọc để trở về toàn bộ danh sách.

```sql
USE EzCare;
SELECT Username, FullName, RoleName, [Password], IsActive, UpdatedAt
FROM dbo.PasswordList
ORDER BY RoleID, Username;
```

- Password gồm UserID liên kết Users, Password và UpdatedAt. PasswordList là view ghép tên đăng nhập/vai trò, không phải database khác.
- Tài khoản cũ chỉ có hash không thể suy ngược mật khẩu. Tài khoản chỉ có hash được bổ sung mật khẩu đọc được khi đăng nhập đúng hoặc đổi/đặt lại mật khẩu.
- Không sửa riêng Password trong SSMS để đổi mật khẩu đăng nhập: dùng giao diện để đồng bộ hash. Admin đặt lại mật khẩu bác sĩ/tiếp nhận ở **Tài khoản nhân sự**; bệnh nhân đổi ở **Hồ sơ cá nhân** với mật khẩu hiện tại.
- Trang tra chỉ dành cho admin. Lưu mật khẩu đọc được là thay đổi theo yêu cầu bài tập, khác NFR04 ban đầu trong Docs vốn yêu cầu lưu hash.

## 4. Ba lớp và các trang

```text
EzCare.sln
├── GUI/        26 trang .aspx cùng .aspx.cs, Site.Master, CSS/JS
├── BLL/        Quyền, kiểm tra dữ liệu và xử lý nghiệp vụ
├── DAL/        SQL, kết nối ADO.NET
├── DTO/        AppointmentDTO, PatientDTO trao đổi dữ liệu
└── Database/   Chỉ có Setup.sql, khởi tạo duy nhất EzCare
```

Luồng **GUI → BLL → DAL → SQL Server**. DTO hỗ trợ trao đổi dữ liệu, không phải lớp nghiệp vụ thứ tư. GUI chỉ tham chiếu BLL/DTO. BasePage dùng chung xác thực, quyền, CSRF, thông báo và định dạng; mỗi trang có LoadData/HandlePost riêng. Designer rỗng là bình thường khi không có server control cần khai báo.

| Yêu cầu | Chức năng | Trang chính |
|---|---|---|
| FR01 | Đăng ký, hồ sơ, đổi mật khẩu | DangKy, HoSoCaNhan |
| FR02 | Đăng nhập/đăng xuất, bốn vai trò | DangNhap, BasePage |
| FR03 | Tạo/sửa/khóa bác sĩ và tiếp nhận | QuanLyNhanSu |
| FR04 | Ca và lượt 15/30 phút, sửa/khóa/mở/hủy ca | QuanLyCa, SuaCa |
| FR05 | Tìm bác sĩ, chuyên khoa và giờ trống | BacSi, DatLich |
| FR06 | Đặt cho mình, đặt hộ, tìm/tạo khách | DatLich, DatLichHo |
| FR07 | Hủy, mở lại giờ, ghi lịch sử | PhieuHen |
| FR08 | Đổi cùng chuyên khoa, giữ mã, rollback khi lỗi | DoiLich |
| FR09 | Tìm mã/SĐT, Đã đến, Không đến | TiepNhan, PhieuHen |
| FR10 | Bác sĩ xem danh sách theo ca, hoàn thành lượt | DanhSachKham, PhieuHen |
| FR11 | Gửi/theo dõi đơn, duyệt/từ chối | CaLamViec, DonDoiCa, DuyetDon |
| FR12 | Lịch bị ảnh hưởng, CSV, liên hệ và dời lịch | LichCanDoi, DoiLich |
| Thông báo | Sự kiện và nhắc hẹn trong web | ThongBao |
| Công khai | Trang chủ, bác sĩ, giới thiệu, liên hệ, điều khoản, bảo mật | Default và trang tương ứng |
| Bổ sung | Chuyên khoa, tra mật khẩu | QuanLyChuyenKhoa, QuanLyMatKhau |

Tên trang trong bảng có đuôi **.aspx**.

## 5. Quy tắc thao tác

- Chỉ chọn ô **Còn trống**. Giờ đã đặt, ca khóa, giờ quá hạn hoặc trùng lịch cá nhân bị vô hiệu hóa. Server kiểm tra lại khi xác nhận.
- Chặn chồng khoảng thời gian của bác sĩ/bệnh nhân kể cả khác SlotID; cho phép hai lượt liền kề. Đặt/đổi/hủy cùng transaction với lịch sử và IsBooked.
- Đổi/hủy phiếu Confirmed trước giờ hẹn ít nhất **24 giờ**, đổi cùng chuyên khoa. Tiếp nhận có ngoại lệ xử lý ca bị khóa có yêu cầu đã duyệt.
- Hủy có lý do và xác nhận ngay trên phiếu, **không popup**. POST chuyển GET bằng HTTP 303 để tải lại không lặp thao tác, cả khi nhập sai.
- Tiếp nhận ghi **Đã đến** đúng ngày, ca mở, trước khi hết lượt; ghi **Không đến** sau giờ kết thúc. Bác sĩ chỉ **Hoàn thành** phiếu của mình đã được tiếp nhận.
- Bác sĩ mặc định thấy ngày ca gần nhất từ hôm nay. Trang đặt khám gợi ý ngày thật sự còn giờ; vẫn đổi bộ lọc để xem ngày khác được.
- Không sửa cấu trúc giờ của ca có phiếu/lịch sử đổi tham chiếu, ca đã qua hoặc đang có đơn chờ duyệt.
- Không đổi chuyên khoa bác sĩ đã có phiếu; không khóa bác sĩ còn phiếu Confirmed/CheckedIn từ hôm nay. Không khóa/duyệt nghỉ ca đang có bệnh nhân đã đến.
- Đặt hộ tái sử dụng hồ sơ cùng họ tên/SĐT/ngày sinh để tránh bản sao. Bệnh nhân xem phiếu của mình, bác sĩ xem phiếu được phân công.

## 6. Giao diện và cấu hình

Google Sans hỗ trợ tiếng Việt, Bootstrap 5.3.3, xanh #0d6efd và nền #f8f9fa. Font/CSS/JS/ảnh nằm trong Content để tải khi không có mạng. Site.Master dùng chung header/menu/footer.

Nút mũi tên nổi ở góc dưới bên phải xuất hiện khi cuộn xuống hơn 320px và ẩn ở đầu trang. Có trên cả trang công khai lẫn các trang sau đăng nhập; không in ra giấy. Nội dung giao diện dùng tên thao tác và thông tin phòng khám, không hiển thị ghi chú bài tập hay thuật ngữ triển khai.

Điền ClinicAddress, ClinicPhone, ClinicEmail trong appSettings của GUI/Web.config để hiển thị liên hệ; chưa điền ghi “Chưa cập nhật”. ReminderHoursBefore mặc định 24, hợp lệ 0–168; 0 tắt nhắc hẹn.

Thông báo và nhắc hẹn hiện trong web khi đăng nhập. Chưa gửi email/SMS hoặc thông báo đẩy khi đóng trình duyệt.

## 7. Trình diễn và xử lý lỗi

1. Patient đặt ngày cách hiện tại ít nhất hai ngày, mở phiếu, đổi cùng chuyên khoa rồi hủy để xem giờ được mở lại.
2. Reception đặt hộ; thử Đã đến bằng phiếu **hôm nay chưa hết giờ**. Phiếu tương lai hiển thị ngày có thể tiếp nhận.
3. Doctor gửi đơn; admin duyệt; reception vào Lịch cần dời, xuất CSV, ghi liên hệ rồi đổi lịch.
4. Doctor hoàn thành phiếu đã tiếp nhận. Patient xem thông báo/trạng thái.
5. Đăng ký tài khoản mới; admin tra Mật khẩu tài khoản; bệnh nhân đổi mật khẩu rồi tải lại danh sách.

| Lỗi | Kiểm tra |
|---|---|
| Không kết nối/Cannot open database | Dịch vụ SQL chạy; Data Source đúng SSMS; Setup chạy trên chính instance đó |
| SQL Login failed | Tài khoản Windows chạy IIS Express có quyền vào SQL Server/EzCare |
| Thiếu Password/IsBooked | Kiểm tra đang kết nối đúng EzCare được khởi tạo bằng Setup.sql của gói này |
| Hết giờ khám | Đúng ngày/chuyên khoa; bác sĩ hoạt động; ca mở. Hết ngày mẫu thì admin tạo ca mới |
| Bác sĩ không thấy lịch | Kiểm tra bác sĩ trên phiếu và bộ lọc ngày/ca/trạng thái |
| Visual Studio không chạy GUI | Chọn GUI làm Startup, Default.aspx làm Start Page, cài workload/.NET 4.8 rồi Build |

Chi tiết kiểm thử xem **KIEM-THU.md**. Gói source không chứa dữ liệu hoặc connection string của database thử.
