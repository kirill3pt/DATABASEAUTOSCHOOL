using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class ModuleAdminWindow : Window
    {

        private readonly CarService carService = new CarService();
        private readonly StudentService studentService = new StudentService();
        private readonly GroupService groupService = new GroupService();
        private readonly PaymentService paymentService = new PaymentService();
        private readonly InstructorService instructorService = new InstructorService();
        private readonly InstructorInvitationService invitationService = new InstructorInvitationService();
        private readonly UserService userService = new UserService();
        private readonly LessonService lessonService = new LessonService();
        private readonly ReportService reportService = new ReportService();
        private readonly InstructorStudentsService studentsService = new InstructorStudentsService();

        private readonly LOGGER logger = new LOGGER();

        public ModuleAdminWindow()
        {
            InitializeComponent();
            logger.LOG("Открыта панель администратора.");

            LoadStudentsCount();
            LoadInstructorsCount();
            LoadLessonsCount();

            Dashboard_Click(null, null);
        }
    }
}
