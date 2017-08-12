using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Laaud_UWP.Models;

namespace Laaud_UWP.Migrations
{
    [DbContext(typeof(MusicLibraryContext))]
    [Migration("20170812131139_AddPlaylists")]
    partial class AddPlaylists
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("Laaud_UWP.Models.Album", b =>
                {
                    b.Property<int>("AlbumId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ArtistId");

                    b.Property<string>("Name");

                    b.HasKey("AlbumId");

                    b.HasIndex("ArtistId");

                    b.ToTable("Albums");
                });

            modelBuilder.Entity("Laaud_UWP.Models.Artist", b =>
                {
                    b.Property<int>("ArtistId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("ArtistId");

                    b.ToTable("Artists");
                });

            modelBuilder.Entity("Laaud_UWP.Models.Playlist", b =>
                {
                    b.Property<int>("PlaylistId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("PlaylistId");

                    b.ToTable("Playlist");
                });

            modelBuilder.Entity("Laaud_UWP.Models.PlaylistItem", b =>
                {
                    b.Property<int>("PlaylistItemId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("OrderInList");

                    b.Property<int>("PlaylistId");

                    b.Property<int>("SongId");

                    b.HasKey("PlaylistItemId");

                    b.HasIndex("PlaylistId");

                    b.HasIndex("SongId");

                    b.ToTable("PlaylistItem");
                });

            modelBuilder.Entity("Laaud_UWP.Models.Song", b =>
                {
                    b.Property<int>("SongId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AlbumId");

                    b.Property<string>("Comment");

                    b.Property<string>("Genre");

                    b.Property<string>("Path");

                    b.Property<int?>("PlaylistId");

                    b.Property<string>("Title");

                    b.Property<int>("Track");

                    b.Property<int>("Year");

                    b.HasKey("SongId");

                    b.HasIndex("AlbumId");

                    b.HasIndex("PlaylistId");

                    b.ToTable("Songs");
                });

            modelBuilder.Entity("Laaud_UWP.Models.Album", b =>
                {
                    b.HasOne("Laaud_UWP.Models.Artist", "Artist")
                        .WithMany("Albums")
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Laaud_UWP.Models.PlaylistItem", b =>
                {
                    b.HasOne("Laaud_UWP.Models.Playlist", "Playlist")
                        .WithMany()
                        .HasForeignKey("PlaylistId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Laaud_UWP.Models.Song", "Song")
                        .WithMany("PlaylistItems")
                        .HasForeignKey("SongId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Laaud_UWP.Models.Song", b =>
                {
                    b.HasOne("Laaud_UWP.Models.Album", "Album")
                        .WithMany("Songs")
                        .HasForeignKey("AlbumId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Laaud_UWP.Models.Playlist")
                        .WithMany("Songs")
                        .HasForeignKey("PlaylistId");
                });
        }
    }
}
