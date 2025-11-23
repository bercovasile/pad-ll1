FROM bitnamilegacy/pgpool:latest

USER root

# Create necessary directories
RUN mkdir -p /var/log/pgpool /var/run/pgpool && \
    chown -R root:root /var/log/pgpool /var/run/pgpool && \
    chmod 755 /var/log/pgpool /var/run/pgpool 

# Copy configuration files
COPY pgpool1.conf /opt/bitnami/pgpool/etc/pgpool1.conf
COPY pgpool2.conf /opt/bitnami/pgpool/etc/pgpool2.conf


# Set proper permissions
RUN chown -R root:root /opt/bitnami/pgpool/etc && \
    chmod 644 /opt/bitnami/pgpool/etc/pgpool1.conf && \
    chmod 644 /opt/bitnami/pgpool/etc/pgpool2.conf

USER root

EXPOSE 5432 9898

CMD ["pgpool", "-n", "-f", "/opt/bitnami/pgpool/etc/pgpool1.conf"]
