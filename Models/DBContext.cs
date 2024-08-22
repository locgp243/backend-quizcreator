using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace backendV1.Models;

public partial class DBContext : DbContext
{
    public DBContext()
    {
    }

    public DBContext(DbContextOptions<DBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AccountQuiz> AccountQuizzes { get; set; }

    public virtual DbSet<Quiz> Quizs { get; set; }

    public virtual DbSet<QuizDetail> QuizDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=Admin_Quizs;TrustServerCertificate=True;Persist Security Info=True;User ID=sa;Password=123456");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Role).HasMaxLength(10);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<AccountQuiz>(entity =>
        {
            entity.ToTable("AccountQuiz");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountQuizzes)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK_AccountQuiz_Accounts");

            entity.HasOne(d => d.Quiz).WithMany(p => p.AccountQuizzes)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("FK_AccountQuiz_Quizs");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.QuizId).HasName("PK__Quizs__8B42AE8E99AD5056");

            entity.Property(e => e.CreateBy).HasMaxLength(50);
            entity.Property(e => e.QuizDesc).HasMaxLength(250);
            entity.Property(e => e.QuizTitle).HasMaxLength(50);
        });

        modelBuilder.Entity<QuizDetail>(entity =>
        {
            entity.HasKey(e => e.QuizDetailId).HasName("PK__QuizDeta__C156E1738FC8DAA1");

            entity.ToTable("QuizDetail");

            entity.Property(e => e.Avatar).HasMaxLength(250);
            entity.Property(e => e.Option1).HasMaxLength(255);
            entity.Property(e => e.Option2).HasMaxLength(255);
            entity.Property(e => e.Option3).HasMaxLength(255);
            entity.Property(e => e.Option4).HasMaxLength(255);
            entity.Property(e => e.Question).HasMaxLength(255);

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizDetails)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("FK__QuizDetai__QuizI__15502E78");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
